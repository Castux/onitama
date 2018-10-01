module View exposing (view)

import Dict
import Game
import Html
import Set


pairRange ( x1, y1 ) ( x2, y2 ) =
    List.range x1 x2
        |> List.concatMap
            (\x ->
                List.range y1 y2
                    |> List.map (Tuple.pair x)
            )


viewCard : Game.State -> Bool -> String -> Html.Html msg
viewCard gameState flip card =
    let
        cardDef =
            Dict.get card gameState.cardDefinitions
                |> Maybe.withDefault []
                |> (if flip then
                        Game.flipCard

                    else
                        identity
                   )
                |> Set.fromList

        rows =
            List.range -2 2
                |> List.map
                    (\row ->
                        Html.tr []
                            (List.range -2 2
                                |> List.map
                                    (\col ->
                                        if Set.member ( col, -row ) cardDef then
                                            Html.td [] [ Html.text "X" ]

                                        else if ( col, row ) == ( 0, 0 ) then
                                            Html.td [] [ Html.text "O" ]

                                        else
                                            Html.td [] [ Html.text "." ]
                                    )
                            )
                    )
    in
    Html.div
        []
        [ Html.table [] rows
        , Html.p [] [ Html.text card ]
        ]


viewBoard grid =
    let
        rows =
            List.range 1 5
                |> List.map
                    (\row ->
                        Html.tr []
                            (List.range 1 5
                                |> List.map
                                    (\col ->
                                        case Dict.get ( col, 6 - row ) grid of
                                            Just ( Game.Student, Game.Top ) ->
                                                Html.td [] [ Html.text "t" ]

                                            Just ( Game.Master, Game.Top ) ->
                                                Html.td [] [ Html.text "T" ]

                                            Just ( Game.Student, Game.Bottom ) ->
                                                Html.td [] [ Html.text "b" ]

                                            Just ( Game.Master, Game.Bottom ) ->
                                                Html.td [] [ Html.text "B" ]

                                            Nothing ->
                                                Html.td [] [ Html.text "." ]
                                    )
                            )
                    )
    in
    Html.table
        []
        rows


viewMove gameState move =
    let
        nextGrid =
            (Game.applyMove gameState move).grid
    in
    Html.div
        []
        [ Html.p [] [ Html.text <| Debug.toString move ]
        , viewBoard nextGrid
        ]


viewMoves gameState =
    Game.validMoves gameState
        |> List.map (viewMove gameState)
        |> Html.div []


view gameState =
    Html.div
        []
        [ Html.h1 [] [ Html.text "Top player" ]
        , gameState.topCards |> Set.toList |> List.map (viewCard gameState True) |> Html.div []
        , Html.h1 [] [ Html.text "Bottom player" ]
        , gameState.bottomCards |> Set.toList |> List.map (viewCard gameState False) |> Html.div []
        , Html.h1 [] [ Html.text "Next card" ]
        , gameState.nextCard |> viewCard gameState False
        , Html.h1 [] [ Html.text "Board" ]
        , gameState.grid |> viewBoard
        , Html.h1 [] [ Html.text "Moves" ]
        , gameState |> viewMoves
        ]
