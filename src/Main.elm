module Main exposing (main)

import Browser
import Game
import Html
import Negamax
import Solver
import View


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
    Html.div []
        [ View.view Game.exampleGame
        , Html.text <| Debug.toString <| state
        ]


main =
    Browser.element
        { init = init
        , update = update
        , subscriptions = subscriptions
        , view = view
        }
