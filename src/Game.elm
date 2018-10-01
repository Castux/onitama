module Game exposing (Card, Pawn(..), Player(..), State, applyMove, exampleGame, flipCard, validMoves)

import Dict exposing (Dict)
import Set exposing (Set)


type Pawn
    = Student
    | Master


type Player
    = Top
    | Bottom


type alias Card =
    List ( Int, Int )


type alias Grid =
    Dict ( Int, Int ) ( Pawn, Player )


type alias State =
    { grid : Grid
    , cardDefinitions : Dict String Card
    , topCards : Set String
    , bottomCards : Set String
    , nextCard : String
    , currentPlayer : Player
    }


type alias Move =
    { card : String
    , origin : ( Int, Int )
    , destination : ( Int, Int )
    }



-- Setup


startingGrid =
    Dict.fromList
        [ ( ( 1, 1 ), ( Student, Bottom ) )
        , ( ( 2, 1 ), ( Student, Bottom ) )
        , ( ( 3, 1 ), ( Master, Bottom ) )
        , ( ( 4, 1 ), ( Student, Bottom ) )
        , ( ( 5, 1 ), ( Student, Bottom ) )
        , ( ( 1, 5 ), ( Student, Top ) )
        , ( ( 2, 5 ), ( Student, Top ) )
        , ( ( 3, 5 ), ( Master, Top ) )
        , ( ( 4, 5 ), ( Student, Top ) )
        , ( ( 5, 5 ), ( Student, Top ) )
        ]


baseCards =
    Dict.fromList
        [ ( "Tiger", [ ( 0, -1 ), ( 0, 2 ) ] )
        , ( "Crab", [ ( -2, 0 ), ( 0, 1 ), ( 2, 0 ) ] )
        , ( "Monkey", [ ( -1, 1 ), ( -1, -1 ), ( 1, 1 ), ( 1, -1 ) ] )
        , ( "Crane", [ ( -1, -1 ), ( 0, 1 ), ( 1, -1 ) ] )
        , ( "Dragon", [ ( -2, 1 ), ( -1, -1 ), ( 1, -1 ), ( 2, 1 ) ] )
        ]


exampleGame : State
exampleGame =
    { grid = startingGrid
    , topCards = Set.fromList [ "Tiger", "Crab" ]
    , bottomCards = Set.fromList [ "Monkey", "Crane" ]
    , nextCard = "Dragon"
    , cardDefinitions = baseCards
    , currentPlayer = Bottom
    }



-- Utils


playerPawnsPositions : Player -> Grid -> List ( Int, Int )
playerPawnsPositions player grid =
    grid
        |> Dict.toList
        |> List.filterMap
            (\( pos, ( pawn, p ) ) ->
                if p == player then
                    Just pos

                else
                    Nothing
            )


validPosition : ( Int, Int ) -> Bool
validPosition ( x, y ) =
    x >= 1 && x <= 5 && y >= 1 && y <= 5


flipCard : Card -> Card
flipCard card =
    List.map (\( x, y ) -> ( -x, -y )) card



-- Moves


validMoves : State -> List Move
validMoves gameState =
    let
        pawnsPos =
            playerPawnsPositions gameState.currentPlayer gameState.grid

        cards =
            case gameState.currentPlayer of
                Top ->
                    gameState.topCards

                Bottom ->
                    gameState.bottomCards

        offsets card =
            Dict.get card gameState.cardDefinitions
                |> Maybe.withDefault []
                |> (case gameState.currentPlayer of
                        Top ->
                            flipCard

                        Bottom ->
                            identity
                   )

        potentialMoves =
            cards
                |> Set.toList
                |> List.concatMap
                    (\cardName ->
                        pawnsPos
                            |> List.concatMap
                                (\( x, y ) ->
                                    offsets cardName
                                        |> List.map
                                            (\( dx, dy ) ->
                                                Move cardName ( x, y ) ( x + dx, y + dy )
                                            )
                                )
                    )

        validMove { card, origin, destination } =
            validPosition destination
                && (case Dict.get destination gameState.grid of
                        Nothing ->
                            True

                        Just ( pawn, player ) ->
                            player /= gameState.currentPlayer
                   )
    in
    List.filter validMove potentialMoves


applyMove : State -> Move -> State
applyMove gameState move =
    let
        grid =
            case Dict.get move.origin gameState.grid of
                Just pawn ->
                    gameState.grid
                        |> Dict.remove move.origin
                        |> Dict.insert move.destination pawn

                Nothing ->
                    gameState.grid

        topCards =
            case gameState.currentPlayer of
                Top ->
                    gameState.topCards
                        |> Set.remove move.card
                        |> Set.insert gameState.nextCard

                Bottom ->
                    gameState.topCards

        bottomCards =
            case gameState.currentPlayer of
                Bottom ->
                    gameState.bottomCards
                        |> Set.remove move.card
                        |> Set.insert gameState.nextCard

                Top ->
                    gameState.bottomCards

        nextCard =
            move.card

        nextPlayer =
            case gameState.currentPlayer of
                Bottom ->
                    Top

                Top ->
                    Bottom
    in
    { gameState
        | grid = grid
        , topCards = topCards
        , bottomCards = bottomCards
        , nextCard = nextCard
        , currentPlayer = nextPlayer
    }
