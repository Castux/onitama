module Main exposing (main)

import Browser
import Game
import Html
import Negamax
import Solver
import View


flip f x y =
    f y x


init : String -> ( ( Float, List Game.Move ), Cmd msg )
init flags =
    let
        result =
            Solver.test
    in
    ( result, Cmd.none )


update msg state =
    ( state, Cmd.none )


subscriptions state =
    Sub.none


view state =
    let
        ( score, moves ) =
            state

        finalPos =
            List.foldl (flip Game.applyMove) Game.exampleGame moves
    in
    Html.div []
        [ View.view Game.exampleGame
        , Html.p [] <| List.singleton <| Html.text <| String.fromFloat score
        , List.map View.moveToText moves |> String.join " / " |> Html.text |> List.singleton |> Html.p []
        , View.viewBoard finalPos.grid
        ]


main =
    Browser.element
        { init = init
        , update = update
        , subscriptions = subscriptions
        , view = view
        }
