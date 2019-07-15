local socket = require "socket"
local onitama = require "onitama"

local function startServer()

	local PORT = 8000
	local server = socket.bind("*", PORT)

	if not server then
		error("Could not start server on port " .. PORT)
	end

	print("Server running on port " .. PORT)

	local clients = {}

	while #clients < 2 do

		local client = server:accept()
		local ip,port = client:getpeername()

		table.insert(clients, client)

		local playerName = #clients == 1 and "Top" or "Bottom"
		print(playerName .. " player connected: " .. ip)
	end

	return clients
end

local function printState(game)

	print "======\n"
	print(onitama.stateToString(game))
	print ""

end

local function startGame(clients)

	math.randomseed(os.time())

	local cardNames = {}
	for k,v in pairs(onitama.Cards) do
		table.insert(cardNames, k)
	end

	local selected = {}
	repeat 
		local i = math.random(#cardNames)
		table.insert(selected, cardNames[i])
		table.remove(cardNames, i)
	until #selected == 5

	local startPlayer = math.random(2)

	print((startPlayer == 1 and "Top" or "Bottom") .. " player starts!")

	local game = onitama.StartState

	game.topCards[1] = selected[1]
	game.topCards[2] = selected[2]
	game.bottomCards[1] = selected[3]
	game.bottomCards[2] = selected[4]
	game.nextCard = selected[5]
	
	game.currentPlayer = startPlayer == 1 and onitama.Top or onitama.Bottom

	for i,client in ipairs(clients) do
		
		local ownCards = i == 1 and game.topCards or game.bottomCards
		local opponentCards = i == 2 and game.topCards or game.bottomCards
		
		local msg =
			"Own: " .. table.concat(ownCards, ",") .. "\n" ..
			"Opponent: " .. table.concat(opponentCards, ",") .. "\n" ..
			"Middle: " .. game.nextCard .. "\n" ..
			(i == startPlayer and "start" or "wait") .. "\n"
		
		client:send(msg)
	end

	printState(game)
end

local function run(clients)

	while true do

		local ready = socket.select(clients, nil, nil, 1)

		for i,client in ipairs(ready) do

			local msg, err = client:receive("*l")

			if err == "closed" then

				print("Player " .. i .. " dropped")
				print("Player " .. (i%2 + 1) .. " wins")
				return

			elseif err == "timeout" then

			else
				print("Player " .. i .. " says: " .. msg)
			end

		end

	end

end

function main()

	local clients = startServer()

	startGame(clients)
	run(clients)

end

main()