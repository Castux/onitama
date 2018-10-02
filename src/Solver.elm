module Solver exposing (test)

import Game
import Negamax


naiveValue state =
    case Game.winner state of
        Just winner ->
            if winner == state.currentPlayer then
                1

            else
                -1

        _ ->
            0


pawnCount state =
    let
        otherPlayer =
            case state.currentPlayer of
                Game.Top ->
                    Game.Bottom

                Game.Bottom ->
                    Game.Top
    in
    case naiveValue state of
        0 ->
            toFloat
                (List.length (Game.pawns state state.currentPlayer)
                    - List.length (Game.pawns state otherPlayer)
                )

        x ->
            toFloat x * 100.0


children state =
    case Game.winner state of
        Just _ ->
            []

        _ ->
            state
                |> Game.validMoves
                |> List.map (\move -> ( move, Game.applyMove state move ))


test =
    Negamax.negamax pawnCount children 7 -100 100 Game.exampleGame
