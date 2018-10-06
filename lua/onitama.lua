-- Utils

local Top = 1
local Bottom = -1

local Student = 1
local Master = 2

local Empty = 0

local samePlayer = function(a,b)
	return a * b > 0
end

local tableCopy
tableCopy = function(t)

	if type(t) ~= "table" then
		return t
	end

	local res = {}
	for k,v in pairs(t) do
		res[k] = tableCopy(v)
	end

	return res
end

-- Cards

local Cards =
{
	Tiger = {{-2,0},{1,0}},
	Crab = {{-1,0},{0,-2},{0,2}},
	Monkey = {{-1,-1},{-1,1},{1,-1},{1,1}},
	Crane = {{-1,0},{1,-1},{1,1}},
	Dragon = {{-1,-2},{-1,2},{1,-1},{1,1}}
}

-- State definition

local StartState =
{
	topCards = {"Tiger", "Crab"},
	bottomCards = {"Monkey", "Crane"},
	nextCard = "Dragon",
	currentPlayer = Bottom,
	grid =
	{
		{1,1,2,1,1},
		{0,0,0,0,0},
		{0,0,0,0,0},
		{0,0,0,0,0},
		{-1,-1,-2,-1,-1}
	}
}

-- Properties

local hasMaster = function(state,player)
	
	for _,line in ipairs(state.grid) do
		for _,cell in ipairs(line) do
			if cell == Master * player then
				return true
			end
		end
	end
	return false
end

local winner = function(state)
	
	if state.grid[1][3] == Bottom * Master then
		return Bottom
		
	elseif state.grid[5][3] == Top * Master then
		return Top
		
	elseif not hasMaster(state,Top) then
		return Bottom
	
	elseif not hasMaster(state,Bottom) then
		return Top
		
	else
		return nil
	end
end

local getPawns = function(state,player) 
	
	local res = {}
	
	for row,line in ipairs(state.grid) do
		for col,cell in ipairs(line) do
			if samePlayer(cell,player) then
				res[#res+1] = {row,col}
			end
		end
	end
	return res
end

local validPosition = function(row,col)
	return row >= 1 and row <= 5 and col >= 1 and col <= 5
end

-- Moves

local validMoves = function(state)
	
	local res = {}
	
	local player = state.currentPlayer
	local cards = player == Top and state.topCards or state.bottomCards
	
	for _,card in ipairs(cards) do
		for _,pawn in ipairs(getPawns(state,player)) do
			for _,offset in ipairs(Cards[card]) do
				
				local orow,ocol = offset[1],offset[2]
				if player == Top then
					orow,ocol = -orow,-ocol
				end
				
				local drow,dcol = pawn[1] + orow, pawn[2] + ocol
				if validPosition(drow,dcol) then
					
					local dcell = state.grid[drow][dcol]
					if not samePlayer(player,dcell) then
						res[#res + 1] =
						{
							card = card,
							from = pawn,
							to = {drow,dcol}
						}
					end
				end
			end
		end		
	end
	
	return res
end

local applyMove = function(state,move)
	
	local destPawn = state.grid[move.to[1]][move.to[2]]
	local origPawn = state.grid[move.from[1]][move.from[2]]
	
	-- Move
	
	state.grid[move.to[1]][move.to[2]] = origPawn
	state.grid[move.from[1]][move.from[2]] = Empty
	
	-- Cards
	
	local cards = state.currentPlayer == Top and state.topCards or state.bottomCards
	for i,c in ipairs(cards) do
		if c == move.card then
			cards[i] = state.nextCard
			break
		end
	end
	
	state.nextCard = move.card
	
	-- Player
	
	state.currentPlayer = -state.currentPlayer
	
	-- Capture
	
	if destPawn ~= 0 then
		return destPawn
	end
end

-- Debug drawing

local pawnToString =
{
	[1] = "t",
	[2] = "T",
	[-1] = "b",
	[-2] = "B"
}

local gridToString = function(grid)
	local res = ""
	for _,line in ipairs(grid) do
		for _,cell in ipairs(line) do
			res = res .. (pawnToString[cell] or ".")
		end
		res = res .. "\n"
	end
	return res
end

local stateToString = function(state)
	local w = winner(state)
	
	return
		table.concat(state.topCards, ",") .. 
		(state.currentPlayer == Top and (",[" .. state.nextCard .. "]") or "") ..
		(w == Top and " WIN" or "") ..
		"\n" ..
		gridToString(state.grid) ..
		table.concat(state.bottomCards, ",") ..
		(state.currentPlayer == Bottom and (",[" .. state.nextCard .. "]") or "") ..
		(w == Bottom and " WIN" or "")
		
end

local moveToString = function(move)
	return move.card .. " " ..
		"(" .. move.from[1] .. "," .. move.from[2] .. ") -> " ..
		"(" .. move.to[1] .. "," .. move.to[2] .. ")"
end


print(stateToString(StartState))
print "===="

for _,m in ipairs(validMoves(StartState)) do
	print(moveToString(m))
	print ""
	local child = tableCopy(StartState)
	applyMove(child,m)
	print(stateToString(child))
	
	print "===="
end