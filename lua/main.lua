local onitama = require "onitama"
local negamax = require "negamax"

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

local state = onitama.StartState

state.grid =
	{
		{0,1,2,1,0},
		{0,0,0,0,0},
		{0,0,0,0,0},
		{0,0,0,0,0},
		{0,-1,-2,-1,0}
	}

--[[
local res, moves = negamax.negamax(naiveValue, childrenFunc, 10, -1, 1, state)
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

---[[
local res, moves = negamax.negamaxInPlace(naiveValue, onitama.validMoves, onitama.applyMove, onitama.undoMove, 12, -1, 1, state)
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

--[[

print(onitama.stateToString(state))
print "===="

for _,m in ipairs(onitama.validMoves(state)) do
	print(onitama.moveToString(m))
	print ""
	local undoData = onitama.applyMove(state, m)
	print(onitama.stateToString(state))
	onitama.undoMove(state, m, undoData)
	
	print "---"
	
	print(onitama.stateToString(state))
	
	
	print "===="
end

--]]