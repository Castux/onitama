module Negamax exposing (Node(..), getValue, negamax, negamax2)

import List.Extra


type Node s m
    = Node
        { state : s
        , children : List ( m, Node s m )
        , value : Float
        }


getValue (Node node) =
    node.value


maximaBy : (item -> Float) -> List item -> ( List item, Maybe Float )
maximaBy f list =
    let
        rec currentMax currentItems rest =
            case ( currentMax, rest ) of
                ( Nothing, x :: xs ) ->
                    rec (Just (f x)) [ x ] xs

                ( Just m, x :: xs ) ->
                    let
                        fx =
                            f x
                    in
                    if fx > m then
                        rec (Just fx) [ x ] xs

                    else if fx == m then
                        rec (Just fx) (x :: currentItems) xs

                    else
                        rec (Just m) currentItems xs

                _ ->
                    ( currentItems, currentMax )
    in
    rec Nothing [] list


negamax valueFunction childrenFunction maxDepth state =
    let
        childrenStates =
            childrenFunction state

        rec =
            negamax valueFunction childrenFunction (maxDepth - 1)
    in
    if maxDepth == 0 || List.length childrenStates == 0 then
        Node
            { state = state
            , children = []
            , value = valueFunction state
            }

    else
        let
            childrenNodes =
                List.map (Tuple.mapSecond rec) childrenStates

            ( best, value ) =
                childrenNodes
                    |> maximaBy (Tuple.second >> getValue >> (*) -1)
        in
        Node
            { state = state
            , children = best
            , value = value |> Maybe.withDefault (-1.0 / 0.0)
            }


negamax2 : (state -> Float) -> (state -> List ( move, state )) -> Int -> state -> ( Float, List move )
negamax2 valueFunction childrenFunction maxDepth state =
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
                                negamax2 valueFunction childrenFunction (maxDepth - 1) child
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
