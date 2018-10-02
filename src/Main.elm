module Main exposing (main)

import Browser
import Game
import Html
import Negamax
import Solver
import View


init : String -> ( Negamax.Node Game.State Game.Move, Cmd msg )
init flags =
    let
        node =
            Solver.test
    in
    ( node, Cmd.none )


update msg state =
    ( state, Cmd.none )


subscriptions state =
    Sub.none


view state =
    let
        (Negamax.Node node) =
            state
    in
    Html.div []
        [ Html.text <| String.fromFloat <| Negamax.getValue state
        , View.view node.state
        ]


main =
    Browser.element
        { init = init
        , update = update
        , subscriptions = subscriptions
        , view = view
        }
