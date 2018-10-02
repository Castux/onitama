module Solver exposing (test)

import Game exposing (EndGame(..))
import Negamax


naiveValue state =
    case Game.endGame state of
        Just (Position winner) ->
            if winner == state.currentPlayer then
                1

            else
                -1

        Just (Capture winner) ->
            if winner == state.currentPlayer then
                1

            else
                -1

        _ ->
            0


children state =
    case Game.endGame state of
        Just _ ->
            []

        _ ->
            state
                |> Game.validMoves
                |> List.map (\move -> ( move, Game.applyMove state move ))


test =
    Negamax.negamax naiveValue children 5 Game.exampleGame
