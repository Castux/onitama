module Main exposing (main)

import Browser
import Game
import Html
import View


init : String -> ( Game.State, Cmd msg )
init flags =
    ( Game.exampleGame, Cmd.none )


update msg state =
    ( state, Cmd.none )


subscriptions state =
    Sub.none


view state =
    Html.div [] [ View.view state ]


main =
    Browser.element
        { init = init
        , update = update
        , subscriptions = subscriptions
        , view = view
        }
