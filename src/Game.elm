module Game exposing (Card, Pawn(..), Player(..), State, exampleGame)

import Dict exposing (Dict)
import Set exposing (Set)


type Pawn
    = Student
    | Master


type Player
    = Top
    | Bottom


type alias Card =
    Set ( Int, Int )


type alias State =
    { grid : Dict ( Int, Int ) ( Pawn, Player )
    , cardDefinitions : Dict String Card
    , topCards : Set String
    , bottomCards : Set String
    , nextCard : String
    , currentPlayer : Player
    }


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
        [ ( "Tiger", Set.fromList [ ( 0, -1 ), ( 0, 2 ) ] )
        , ( "Crab", Set.fromList [ ( -2, 0 ), ( 0, 1 ), ( 2, 0 ) ] )
        , ( "Monkey", Set.fromList [ ( -1, 1 ), ( -1, -1 ), ( 1, 1 ), ( 1, -1 ) ] )
        , ( "Crane", Set.fromList [ ( -1, -1 ), ( 0, 1 ), ( 1, -1 ) ] )
        , ( "Dragon", Set.fromList [ ( -2, 1 ), ( -1, -1 ), ( 1, -1 ), ( 2, 1 ) ] )
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
