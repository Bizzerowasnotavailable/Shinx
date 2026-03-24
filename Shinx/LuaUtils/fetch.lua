shinx.register("fetch", function()
    local success, err = pcall(function()
        shinx.color("yellow")

        local art = {
            "                                     YXXXXXXY                            ",
            "                                  YXXXXXXXXXXXXY                         ",
            "                                 XWHXXXXXFFXXXXXX                        ",
            "                                YXIXXXXXXEXXXXXXXX                       ",
            "                                XXXXXXXXXXXXXXXWVU                       ",
            "                                RSSSSSSSSSSVWVUUTS                       ",
            "                                NMMLLLLLLMNMTTSRQRXXXX                   ",
            "                                 QQRRRRQQPLRRQQPQXXXXXXX                 ",
            "                                YXYMLMMLLPPPPPXXXXXXXXXXXY               ",
            "                               XXXXXXXXXXXXXXXXXXXXXXXXXWW               ",
            "                              YXXXXXXXXXXXXXXXXXXXXXXWWVUS               ",
            "                              WXXXXXXXXXXXXXXXXXWWVVUTTSQW               ",
            "                               TTUVVVWWWWWVVVVUUTTSRRQQQY                ",
            "                                RRRRRRRRRRRRRQQQPPPPPQS                  ",
            "                                  RQQQQPPPPPPPPPPPPQ                     ",
            "                                       SQQQQQQY                          "
        }

        for _, line in ipairs(art) do shinx.writeline(line) end

        shinx.writeline("")
        shinx.writeline("OS: barebones SHINX")
        shinx.writeline("Kernel: SHINX")

        local ok1, cpu = pcall(shinx.fetchcpu)
        if not ok1 then cpu = "idk" end
        shinx.writeline("CPU: " .. cpu)

        local ok2, ram = pcall(shinx.fetchram)
        if not ok2 then ram = "idk" end
        shinx.writeline("RAM: " .. ram)

        shinx.writeline("Resolution: currently in console mode")

        shinx.resetcolor()
    end)
    if not success then
        shinx.resetcolor()
        shinx.writeline("error in fetch: " .. err)
    end
end, "show system info")