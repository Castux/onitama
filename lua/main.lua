local onitama = require "onitama"
local negamax = require "negamax"

local function cardCombinations()
	
	local res = {}
	local c = {}
	
	for k,v in pairs(onitama.Cards) do
		table.insert(c,k)
	end
	
	local taken = {}
	
	for i = 1,#c do
		taken[c[i]] = true
		for j = i+1,#c do
			taken[c[j]] = true
			
			for u = 1,#c do
				if not taken[c[u]] then
					taken[c[u]] = true
					for v = u+1,#c do
						if not taken[c[v]] then
						taken[c[v]] = true
							
							for k = 1,#c do
								if not taken[c[k]] then
									table.insert(res, {c[i],c[j],c[u],c[v],c[k]})
								end
							end
							
						taken[c[v]] = nil
						end
					end
					taken[c[u]] = nil
				end
			end
			
			taken[c[j]] = nil
		end
		taken[c[i]] = nil
	end
	
	return res
end

local function childrenFunc(state)
	
	local res = {}
	for _,move in ipairs(onitama.validMoves(state)) do
		
		local child = onitama.copyState(state)
		onitama.applyMove(child, move)
		
		res[#res + 1] = {move, child}
	end
	
	return res
end

local function naiveValue(state)
	
	local winner = onitama.getWinner(state)
	
	if winner == state.currentPlayer then
		return 1
	elseif winner == -state.currentPlayer then
		return -1
	else
		return 0
	end
end

local function pawnDifference(state)
	
	local diff = 0
	for _,line in ipairs(state.grid) do
		for _,cell in ipairs(line) do
			if cell ~= 0 then
				diff = diff + (onitama.samePlayer(cell, state.currentPlayer) and 1 or -1)
			end
		end		
	end
	
	return diff
end


local function smart(state)
	
	local naive = naiveValue(state)
	if naive ~= 0 then
		return naive * 100
	end
	
	return pawnDifference(state)
end
--[[
local state = onitama.StartState

state.grid =
	{
		{0,0,2,0,0},
		{0,0,0,0,0},
		{0,0,0,0,0},
		{0,0,0,0,0},
		{0,0,-2,0,0}
	}



local res, moves = negamax.negamaxInPlace(smart, onitama.validMoves, onitama.applyMove, onitama.undoMove, 12, -100, 100, state)
print(res)

print(onitama.stateToString(state))

for i = #moves,1,-1 do
	
	print "==="
	local m = moves[i]
	print(onitama.moveToString(moves[i]))
	onitama.applyMove(state,m)
	print(onitama.stateToString(state))
end
--]]

for i,cards in ipairs(cardCombinations()) do

	local state = onitama.copyState(onitama.StartState)
	state.grid =
		{
			{0,0,2,0,0},
			{0,0,0,0,0},
			{0,0,0,0,0},
			{0,0,0,0,0},
			{0,0,-2,0,0}
		}

	state.topCards[1] = cards[1]
	state.topCards[2] = cards[2]
	state.bottomCards[1] = cards[3]
	state.bottomCards[2] = cards[4]
	state.nextCard = cards[5]
	
	local score,moves = negamax.negamaxInPlace(smart, onitama.validMoves, onitama.applyMove, onitama.undoMove, 6, -100, 100, state)
	
	print(i, table.concat(cards, ","), score, onitama.moveToString(moves[#moves]))
end
