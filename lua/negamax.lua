local function negamax(valueFunction, childrenFunction, depth, alpha, beta, node)
	
	local children = childrenFunction(node)
	
	if depth == 0 or #children == 0 then
		return valueFunction(node), {}
	end
	
	local value = -math.huge
	local moves
	
	for _,pair in ipairs(children) do
		
		local move, child = pair[1], pair[2]
		
		local childValue, nextMoves = negamax(valueFunction, childrenFunction, depth - 1, -beta, -alpha, child)
		
		childValue = -childValue
		nextMoves[#nextMoves + 1] = move
		
		if childValue > value then
			value = childValue
			moves = nextMoves
		end
		
		alpha = math.max(childValue, alpha)
		
		if alpha >= beta then
			break
		end		
	end
	
	return value, moves
end

local function negamaxInPlace(valueFunction, movesFunction, applyMove, undoMove, depth, alpha, beta, node)
	
	local moves = movesFunction(node)
	
	if depth == 0 or #moves == 0 then
		return valueFunction(node), {}
	end
	
	local value = -math.huge
	local bestMoves
	
	for _,move in ipairs(moves) do
		
		applyMove(node, move)
		
		local childValue, nextMoves = negamaxInPlace(valueFunction, movesFunction, applyMove, undoMove, depth - 1, -beta, -alpha, node)
		
		undoMove(node, move)

		childValue = -childValue
		nextMoves[#nextMoves + 1] = move
		
		if childValue > value then
			value = childValue
			bestMoves = nextMoves
		end
		
		alpha = math.max(childValue, alpha)
		
		if alpha >= beta then
			break
		end		
	end
	
	return value, bestMoves
end


return
{
	negamax = negamax,
	negamaxInPlace = negamaxInPlace
}