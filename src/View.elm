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


viewCard : Game.State -> String -> Html.Html msg
viewCard gamestate card =
    let
        cardDef =
            Dict.get card gamestate.cardDefinitions
                |> Maybe.withDefault Set.empty

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
                                            Html.td [] [ Html.text " " ]
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


view gamestate =
    Html.div
        []
        [ Html.h1 [] [ Html.text "Top player" ]
        , gamestate.topCards |> Set.toList |> List.map (viewCard gamestate) |> Html.div []
        , Html.h1 [] [ Html.text "Bottom player" ]
        , gamestate.bottomCards |> Set.toList |> List.map (viewCard gamestate) |> Html.div []
        , Html.h1 [] [ Html.text "Next card" ]
        , gamestate.nextCard |> viewCard gamestate
        , Html.h1 [] [ Html.text "Board" ]
        , gamestate.grid |> viewBoard
        ]
