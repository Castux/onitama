local socket = require "socket"
local onitama = require "onitama"

local function playerName(player)
	return player == onitama.Top and "Top" or "Bottom"
end

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

		local player = #clients == 1 and onitama.Top or onitama.Bottom
		clients[client] = player

		print(playerName(player) .. " player connected: " .. ip)
		client:send("You are " .. playerName(player) .. "\n")
	end

	return clients
end

local function printState(game)

	print "======\n"
	print(onitama.stateToString(game))
	print ""

	for _,move in ipairs(onitama.validMoves(game)) do
		print(onitama.moveToString(move))
	end

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

	local game = onitama.StartState

	game.topCards[1] = selected[1]
	game.topCards[2] = selected[2]
	game.bottomCards[1] = selected[3]
	game.bottomCards[2] = selected[4]
	game.nextCard = selected[5]
	
	game.currentPlayer = startPlayer == 1 and onitama.Top or onitama.Bottom

	local msg = "Top: " .. table.concat(game.topCards, ",") .. "\n" ..
			"Bottom: " .. table.concat(game.bottomCards, ",") .. "\n" ..
			"Middle: " .. game.nextCard .. "\n" ..
			playerName(game.currentPlayer) .. " starts\n"

	for i,client in ipairs(clients) do
		client:send(msg)
	end
	
	print(playerName(game.currentPlayer) .. " starts")
	printState(game)
	
	return game
end

local columnNames =
{
	a = 1,
	b = 2,
	c = 3,
	d = 4,
	e = 5
}

local function handleInput(game, from, msg)
	
	-- Check current player
	
	if game.currentPlayer ~= from then
		print(playerName(from) .. " player (not their turn) says: " .. msg)
		return
	end
	
	-- Parse input
	
	local card, ocol, orow, dcol, drow = msg:match("^(%w+) (%a)(%d) (%a)(%d)$")
	
	if not (card and ocol and orow and dcol and drow) then
		print(playerName(from) .. " player (on their turn) says: " .. msg)	
		return
	end
	
	-- Columns to numbers
	
	ocol = columnNames[ocol]
	dcol = columnNames[dcol]
	
	-- Move validity
	
	for _,move in ipairs(onitama.validMoves(game)) do
		
		if move.card == card and
			move.from[1] == orow and
			move.from[2] == ocol and
			move.to[1] == drow and
			move.to[2] == dcol then
			
			print(playerName(from) .. " player plays ", msg)
			
			onitama.applyMove(game, move)
			printState(game)
			
			for _,client in ipairs(clients) do
				client:send(msg)
			end
			
			if getWinner(state) then
				return "abort"
			end
			
			return
		end
	end
	
	print(playerName(from) .. " player submitted an invalid move: ", msg)
	return "abort"
end

local function run(game, clients)

	while true do

		local ready = socket.select(clients, nil, nil, 1)

		for i,client in ipairs(ready) do

			local msg, err = client:receive("*l")

			if err == "closed" then

				print(playerName(clients[client]) .. " player dropped")
				return

			elseif err == "timeout" then

			else
				local status = handleInput(game, clients[client], msg)
				
				if status == "abort" then
					return
				end
			end

		end

	end

end

function main()

	local clients = startServer()

	local game = startGame(clients)
	run(game, clients)

end

main()