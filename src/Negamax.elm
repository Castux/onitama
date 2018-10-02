module Negamax exposing (negamax)

import List.Extra


negamax : (state -> Float) -> (state -> List ( move, state )) -> Int -> state -> ( Float, List move )
negamax valueFunction childrenFunction maxDepth state =
    let
        terminal =
            ( valueFunction state, [] )
    in
    if maxDepth == 0 then
        terminal

    else
        let
            helper currentMax result list =
                case list of
                    [] ->
                        ( currentMax, result )

                    ( move, child ) :: xs ->
                        let
                            ( value, nextMoves ) =
                                negamax valueFunction childrenFunction (maxDepth - 1) child
                                    |> Tuple.mapFirst ((*) -1)
                        in
                        if value > currentMax then
                            helper value (move :: nextMoves) xs

                        else
                            helper currentMax result xs
        in
        case childrenFunction state of
            [] ->
                terminal

            list ->
                helper (-1.0 / 0.0) [] list
