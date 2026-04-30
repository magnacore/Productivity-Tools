-- ==============================================================================
-- INITIALIZATION
-- ==============================================================================
-- Default Leader is used which is \
vim.g.mapleader = "\\"

-- ==============================================================================
-- PLUGINS
-- ==============================================================================
-- Using vim.cmd allows seamless use of vim-plug in init.lua
vim.cmd([[
  call plug#begin(stdpath('data') . '/plugged')

  " Fuzzy finder
  Plug 'junegunn/fzf', { 'do': { -> fzf#install() } }
  Plug 'junegunn/fzf.vim'

  " Jump to anywhere in file
  Plug 'easymotion/vim-easymotion'

  " Fuzzy search
  Plug 'haya14busa/incsearch.vim'
  Plug 'haya14busa/incsearch-fuzzy.vim'
  Plug 'haya14busa/incsearch-easymotion.vim'

  " Surround objects
  Plug 'tpope/vim-surround'

  " Replace motion with register contents
  Plug 'inkarkat/vim-ReplaceWithRegister'

  " File explorer
  Plug 'preservim/nerdtree'

  " Comment with motion
  Plug 'tpope/vim-commentary'

  " Show hex colors
  " Plug 'rrethy/vim-hexokinase', { 'do': 'make hexokinase' }

  " Convert text to title case using motion
  Plug 'christoomey/vim-titlecase'

  " Color themes
  " Plug 'morhetz/gruvbox'
  " Plug 'sickill/vim-monokai'
  Plug 'phanviet/vim-monokai-pro'
  " Plug 'overcache/NeoSolarized'

  " Physics based scrolling
  " Plug 'yuttie/comfortable-motion.vim'

  " Status bar
  Plug 'vim-airline/vim-airline'
  Plug 'vim-airline/vim-airline-themes'

  " For linters and fixers
  " Plug 'dense-analysis/ale'

  " Autocomplete code
  " Plug 'neoclide/coc.nvim'

  " Development icons
  Plug 'ryanoasis/vim-devicons'

  " multiple cursors
  Plug 'terryma/vim-multiple-cursors'

  " Vim Terminal
  " Plug 'tc50cal/vim-terminal'

  " Tagbar for code navigation
  " Plug 'preservim/tagbar'

  " Open buffers in tabs
  Plug 'ap/vim-buftabline'

  " Git
  Plug 'tiagofumo/vim-nerdtree-syntax-highlight'
  Plug 'Xuyuanp/nerdtree-git-plugin'
  Plug 'airblade/vim-gitgutter'

  " Markdown
  Plug 'nvim-treesitter/nvim-treesitter', {'do': ':TSUpdate'}
  Plug 'nvim-tree/nvim-web-devicons' " Optional, for icons
  Plug 'MeanderingProgrammer/render-markdown.nvim'

  call plug#end()
]])

-- ==============================================================================
-- VIM SETTINGS
-- ==============================================================================
local opt = vim.opt

opt.ruler = true
opt.number = true
opt.relativenumber = true
opt.showcmd = true
opt.showmode = true

-- Enable autocompletion for commands
opt.wildmenu = true
opt.wildmode = { "list:longest", "full" }

-- Autocomplete words
opt.complete:append("kspell")
opt.completeopt = { "menuone", "longest" }

-- Search settings
opt.ignorecase = true  -- Searches are case insensitive
opt.incsearch = true   -- Scroll to search
opt.smartcase = true   -- if lowercase, insensitive; else case sensitive
opt.hlsearch = true    -- Highlight search

-- Wrap text
opt.wrap = true
opt.linebreak = true   -- Do not break wrap in the middle of words (lbr)

-- Indentation
opt.autoindent = true
opt.smartindent = true

-- Set color scheme
opt.bg = "dark"
vim.cmd("syntax enable")
vim.cmd("colorscheme monokai_pro")
-- vim.cmd("colorscheme NeoSolarized")

vim.g.airline_theme = "molokai"
-- vim.g.airline_theme = "solarized"
-- vim.g.airline_theme = "base16_monokai"

if vim.fn.has("termguicolors") == 1 then
    opt.termguicolors = true
end

-- Make the background transparent
vim.api.nvim_set_hl(0, "Normal", { bg = "none", ctermbg = "none" })
vim.api.nvim_set_hl(0, "NonText", { bg = "none", ctermbg = "none" })

opt.history = 1000
opt.scrolloff = 5
opt.tabstop = 4

-- Splits open at the bottom and right
opt.splitbelow = true
opt.splitright = true

opt.encoding = "utf-8"
opt.fileformat = "unix"
opt.title = true
opt.signcolumn = "yes"

-- Enable clipboard
opt.clipboard:append("unnamedplus")

-- Spelling languages
opt.spelllang = { "en", "cjk" }
opt.spell = true

-- ==============================================================================
-- CUSTOM SHORTCUTS
-- ==============================================================================
local keymap = vim.keymap.set
local noremap = { noremap = true }
local silent = { silent = true }
local remap = { remap = true }

-- Toggle Highlight
keymap("n", "<Leader>h", ":nohl<CR>", noremap)

-- Window Navigation
keymap("n", "<C-h>", "<C-w>h", noremap)
keymap("n", "<C-j>", "<C-w>j", noremap)
keymap("n", "<C-k>", "<C-w>k", noremap)
keymap("n", "<C-l>", "<C-w>l", noremap)

-- Make adjusting split sizes a bit more friendly
keymap("n", "<C-Left>", ":vertical resize +3<CR>", noremap)
keymap("n", "<C-Right>", ":vertical resize -3<CR>", noremap)
keymap("n", "<C-Up>", ":resize +3<CR>", noremap)
keymap("n", "<C-Down>", ":resize -3<CR>", noremap)

-- Change 2 split windows from vert to horiz or horiz to vert
keymap("n", "<Leader>th", "<C-w>t<C-w>H", { remap = true })
keymap("n", "<Leader>tk", "<C-w>t<C-w>K", { remap = true })

-- Open Xonsh shell
keymap("n", "<Leader>tt", ":new term://~/anaconda3/envs/xonsh/bin/xonsh --rc ~/.xonshrc<CR>", { remap = true })

-- Spell Checking Toggle
keymap("n", "<F10>", ":set spell!<cr>", silent)
keymap("i", "<F10>", "<C-O>:set spell!<cr>", silent)
keymap("i", "<C-s>", "<c-g>u<Esc>[s1z=`]a<c-g>u", noremap)

-- Move selection
keymap("v", "<A-j>", ":m '>+1<CR>gv=gv", noremap)
keymap("v", "<A-k>", ":m '<-2<CR>gv=gv", noremap)

-- Open registers
keymap("n", "<Leader>c", ":reg<CR>", noremap)

-- FZF shortcuts (opens in full screen)
keymap("n", "<Leader>ff", ":Files!<CR>", noremap)
keymap("n", "<Leader>fl", ":Lines!<CR>", noremap)
keymap("n", "<Leader>f/", ":BLines!<CR>", noremap)
keymap("n", "<Leader>fm", ":Marks!<CR>", noremap)
keymap("n", "<Leader>fw", ":Windows!<CR>", noremap)
keymap("n", "<Leader>fr", ":Rg!<CR>", noremap)
keymap("n", "<Leader>fc", ":Colors!<CR>", noremap)
keymap("n", "<Leader>fh", ":History!<CR>", noremap)
keymap("n", "<Leader>fx", ":Commands!<CR>", noremap)
keymap("n", "<Leader>fb", ":Buffers!<CR>", noremap)

-- ==============================================================================
-- PLUGIN SETTINGS
-- ==============================================================================
-- Easymotion
keymap({ "n", "v", "o" }, "<Leader>gf", "<Plug>(easymotion-bd-f)", remap)
keymap("n", "<Leader>gf", "<Plug>(easymotion-overwin-f)", remap)

keymap("n", "<Leader>gs", "<Plug>(easymotion-overwin-f2)", remap)

keymap({ "n", "v", "o" }, "<Leader>gl", "<Plug>(easymotion-bd-jk)", remap)
keymap("n", "<Leader>gl", "<Plug>(easymotion-overwin-line)", remap)

keymap({ "n", "v", "o" }, "<Leader>gw", "<Plug>(easymotion-bd-w)", remap)
keymap("n", "<Leader>gw", "<Plug>(easymotion-overwin-w)", remap)

-- Replace With Register
keymap("n", "<Leader>r", "<Plug>ReplaceWithRegisterOperator", remap)
keymap("n", "<Leader>rr", "<Plug>ReplaceWithRegisterLine", remap)
keymap("x", "<Leader>r", "<Plug>ReplaceWithRegisterVisual", remap)

-- ALE
vim.g.ale_linters = { python = { 'pylint' } }
vim.g.ale_fixers = { ['*'] = { 'remove_trailing_lines', 'trim_whitespace' }, python = { 'black', 'isort' } }
vim.g.ale_fix_on_save = 1

-- Nerdtree
keymap("n", "<A-f>", ":NERDTreeFocus<CR>", noremap)
keymap("n", "<A-t>", ":NERDTreeToggle<CR>", noremap)
keymap("n", "<A-l>", ":call CocActionAsync('jumpDefinition')<CR>", noremap)

vim.g.NERDTreeGitStatusWithFlags = 1
vim.g.NERDTreeIgnore = { '^node_modules$' }

-- Tagbar
keymap("n", "<F8>", ":TagbarToggle<CR>", remap)

-- Vim bufftagline
keymap("n", "<Leader>bn", ":bnext<CR>", noremap)
keymap("n", "<Leader>bp", ":bprev<CR>", noremap)

-- ==============================================================================
-- FUNCTIONS, MACROS & ABBREVIATIONS
-- ==============================================================================
vim.cmd([[
  " Incsearch Configuration
  function! s:incsearch_config(...) abort
    return incsearch#util#deepextend(deepcopy({
          \   'modules': [incsearch#config#easymotion#module({'overwin': 1})],
          \   'keymap': {
          \     "\<CR>": '<Over>(easymotion)'
          \   },
          \   'is_expr': 0
          \ }), get(a:, 1, {}))
  endfunction

  function! s:config_easyfuzzymotion(...) abort
    return extend(copy({
    \   'converters': [incsearch#config#fuzzyword#converter()],
    \   'modules': [incsearch#config#easymotion#module({'overwin': 1})],
    \   'keymap': {"\<CR>": '<Over>(easymotion)'},
    \   'is_expr': 0,
    \   'is_stay': 1
    \ }), get(a:, 1, {}))
  endfunction

  noremap <silent><expr> / incsearch#go(<SID>incsearch_config())
  noremap <silent><expr> <space>/  incsearch#go(<sid>config_easyfuzzymotion())
  noremap <silent><expr> ? incsearch#go(<SID>incsearch_config({'command': '?'}))
  noremap <silent><expr> g/ incsearch#go(<SID>incsearch_config({'is_stay': 1}))

  " Add incrementing numbers
  let @i ='let i = 1 | g/^/s/^/\=printf("%03d ", i)/ | let i = i+1'

  " Remove NA - from lines
  let @r ='gg G4lx'

  " Abbreviations
  ab mcs Manuj Chandra Sharma
  ab gai Generative AI
  ab config configuration
]])

-- ==============================================================================
-- TREESITTER & MARKDOWN SETUP
-- ==============================================================================

-- 1. Initialize Treesitter and define ensure_installed
local ts_status, ts_configs = pcall(require, "nvim-treesitter.configs")
if ts_status then
    ts_configs.setup({
        -- This is where you list all the languages you want automatically installed!
        ensure_installed = { "markdown", "markdown_inline", "python", "bash", "yaml", "json", "lua", "vim", "c_sharp" },

        highlight = {
            enable = true, -- This is the magic switch that actually turns on the colors
            additional_vim_regex_highlighting = false,
        },
    })
end

-- 2. Initialize your render-markdown plugin
local rm_status, render_markdown = pcall(require, "render-markdown")
if rm_status then
    render_markdown.setup({})

    -- conceallevel is required by render-markdown to hide the backticks and formatting symbols
    vim.opt.conceallevel = 2
end
