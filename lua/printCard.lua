local onitama = require "onitama"

for k,v in pairs(onitama.Cards) do
	
	local grid = {
	{".", ".", ".", ".", "."},	
	{".", ".", ".", ".", "."},
	{".", ".", "O", ".", "."},
	{".", ".", ".", ".", "."},
	{".", ".", ".", ".", "."},
	}
	
	for _,offset in ipairs(v) do
		
		grid[3 + offset[1]][3 + offset[2]] = "X"
		
	end
	
	print(k)
	
	for i,v in ipairs(grid) do
		print(table.concat(v))
	end
	
	print ""
	
end