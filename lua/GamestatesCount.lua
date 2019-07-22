function combinations(n,k)
	
	local prod = 1
	
	for i = n, n-k+1, -1 do
		prod = prod * i
	end
	
	for i = 1,k do
		prod = prod / i
	end
	
	return prod
end

function boardStates()
	
	local count = 0
	
	for i = 0,4 do
		for j = 0,4 do
			
			-- Player 1 pawns
			local pawns = combinations(25,i)
			
			-- Player 2 pawns
			pawns = pawns * combinations(25 - i,j)
			
			local masters =
				(25 - i - j) * (25 - i - j - 1) -- both have masters
				+ (25 - i - j)					-- only player 1 has a master
				+ (25 - i - j)					-- only player 2 has a master
			
			count = count + pawns * masters
		end
	end
	
	return count
end

function cardStates()

	-- First player has 2 cards from 5, middle card is 1 among 3
	-- Second player has no choice left
	
	return combinations(5,2) * 3
	
end

local bs = boardStates()
local cs = cardStates()

print("Board", bs, math.log(bs,2) .. " bits")
print("Cards", cs, math.log(cs,2) .. " bits")
print("Game", bs * cs, math.log(bs * cs, 2) .. " bits")