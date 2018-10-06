local function negamaxInPlace(valueFunction, movesFunction, applyMove, undoMove, depth, alpha, beta, node)
	
	local moves = movesFunction(node)
	
	if depth == 0 or #moves == 0 then
		return valueFunction(node)
	end
	
	local value = -math.huge
	local bestMove
	
	for _,move in ipairs(moves) do
		
		applyMove(node, move)
		
		local childValue = negamaxInPlace(valueFunction, movesFunction, applyMove, undoMove, depth - 1, -beta, -alpha, node)
		
		undoMove(node, move)

		childValue = -childValue
		
		if childValue > value then
			value = childValue
			bestMove = move
		end
		
		alpha = math.max(childValue, alpha)
		
		if alpha >= beta then
			break
		end		
	end
	
	return value, bestMove
end


return
{
	negamax = negamaxInPlace
}