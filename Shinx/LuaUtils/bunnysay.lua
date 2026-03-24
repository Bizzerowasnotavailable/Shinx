shinx.register("bunnysay", function()
    local args = shinx.args()

    if #args == 0 then
        shinx.writeline("bunnysay")
        shinx.writeline("like cowsay, but way worse, now on SHINX")
        shinx.writeline("usage: bunnysay <message>")
        return
    end

    local input = table.concat(args, " ")
    local length = #input

    -- logic for the textbox that dynamically changes in size
    local line = string.rep("-", length + 4)

    shinx.writeline(line)
    shinx.writeline("| " .. input .. " |")
    shinx.writeline(line)
    shinx.writeline("^ ^      ||")
    shinx.writeline("(0w0) <3 ||")
    shinx.writeline("/> >|    ||")
    shinx.writeline(" ")
    shinx.writeline("by Bizzero") -- removed the year cuz yeah
end, "port of bunnysay utility")