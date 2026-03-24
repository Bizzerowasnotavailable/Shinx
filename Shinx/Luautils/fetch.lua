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

        for _, line in ipairs(art) do shinx.print(line) end

        shinx.print("")
        shinx.print("OS: barebones SHINX")
        shinx.print("Kernel: SHINX")

        local ok1, cpu = pcall(shinx.fetchcpu)
        if not ok1 then cpu = "idk" end
        shinx.print("CPU: " .. cpu)

        local ok2, ram = pcall(shinx.fetchram)
        if not ok2 then ram = "idk" end
        shinx.print("RAM: " .. ram)

        shinx.print("Resolution: currently in console mode")

        shinx.resetcolor()
    end)
    if not success then
        shinx.resetcolor()
        shinx.print("error in fetch: " .. err)
    end
end, "show system info")