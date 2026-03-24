shinx.register("bunnysay", function()
    local args = shinx.args()

    if #args == 0 then
        shinx.print("bunnysay")
        shinx.print("like cowsay, but way worse, now on SHINX")
        shinx.print("usage: bunnysay <message>")
        return
    end

    local input = table.concat(args, " ")
    local length = #input

    -- logic for the textbox that dynamically changes in size
    local line = string.rep("-", length + 4)

    shinx.print(line)
    shinx.print("| " .. input .. " |")
    shinx.print(line)
    shinx.print("^ ^      ||")
    shinx.print("(0w0) <3 ||")
    shinx.print("/> >|    ||")
    shinx.print(" ")
    shinx.print("by Bizzero") -- removed the year cuz yeah
end, "port of bunnysay utility")