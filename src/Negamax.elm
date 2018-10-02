module Negamax exposing (negamax)

import List.Extra


negamax : (state -> Float) -> (state -> List ( move, state )) -> Int -> Float -> Float -> state -> ( Float, List move )
negamax valueFunction childrenFunction maxDepth alpha beta state =
    let
        terminal =
            ( valueFunction state, [] )
    in
    if maxDepth == 0 then
        terminal

    else
        let
            helper currentMax result currentAlpha list =
                case list of
                    [] ->
                        ( currentMax, result )

                    ( move, child ) :: xs ->
                        let
                            ( value, nextMoves ) =
                                negamax valueFunction childrenFunction (maxDepth - 1) -beta -alpha child
                                    |> Tuple.mapFirst ((*) -1)

                            ( newMax, newResult ) =
                                if value > currentMax then
                                    ( value, move :: nextMoves )

                                else
                                    ( currentMax, result )

                            newAlpha =
                                max currentAlpha value
                        in
                        if newAlpha >= beta then
                            ( newMax, newResult )

                        else
                            helper newMax newResult newAlpha xs
        in
        case childrenFunction state of
            [] ->
                terminal

            list ->
                helper (-1.0 / 0.0) [] alpha list
