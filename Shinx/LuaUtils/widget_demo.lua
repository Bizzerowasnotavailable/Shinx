-- widget demo

local progress_val = 0
local log_text = "App started.\n"

shinx.gui_register(
    "widgetdemo",
    "Widget Demo",
    "__wd_draw",
    "__wd_key",
    120, 80,
    460, 380
)

function __wd_draw()
    local w = shinx.gui_width()
    local h = shinx.gui_height()

    shinx.gui_rectfill(0, 0, w, h, "0,0,60")

    shinx.gui_label(10, 8, "Widget Demo", "yellow")
    shinx.gui_line(0, 24, w, 24, "gray")

    shinx.gui_label(10, 34, "Name:", "white")
    local name = shinx.gui_input("name_field", 60, 30, 180, "enter name...")

    if shinx.gui_button(250, 30, 100, 22, "Greet", "darkblue") then
        if name ~= "" then
            log_text = log_text .. "Hello, " .. name .. "!\n"
        else
            log_text = log_text .. "Please enter a name.\n"
        end
    end

    shinx.gui_label(10, 65, "Options:", "white")
    local loud    = shinx.gui_checkbox("opt_loud",    10, 82, "Loud mode")
    local verbose = shinx.gui_checkbox("opt_verbose", 10, 104, "Verbose")

    if shinx.gui_button(200, 82, 120, 22, "Log status", "0,80,0") then
        local msg = "Status: "
        if loud    then msg = msg .. "[LOUD] " end
        if verbose then msg = msg .. "[VERBOSE] " end
        if not loud and not verbose then msg = msg .. "normal" end
        log_text = log_text .. msg .. "\n"
    end

    shinx.gui_label(10, 136, "Progress:", "white")
    shinx.gui_progressbar(10, 154, w - 20, 22, progress_val, 100, "cyan")

    if shinx.gui_button(10, 184, 80, 22, "+10", "gray") then
        progress_val = math.min(100, progress_val + 10)
    end
    if shinx.gui_button(100, 184, 80, 22, "Reset", "darkred") then
        progress_val = 0
    end

    shinx.gui_line(0, 215, w, 215, "gray")

    shinx.gui_label(10, 220, "Log:", "white")
    shinx.gui_textbox(10, 238, w - 20, h - 248, log_text, "lightgray", "0,0,40")
end

function __wd_key(key)
    if key.key == "Escape" then
        shinx.gui_close("widgetdemo")
    end
end
