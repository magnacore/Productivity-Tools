"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" PLUGINS
"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
call plug#begin('~/.vim/plugged')

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
Plug 'rrethy/vim-hexokinase', { 'do': 'make hexokinase' }

" Convert text to title case using motion
Plug 'christoomey/vim-titlecase'

" Color themes
Plug 'morhetz/gruvbox'
Plug 'sickill/vim-monokai'

" Physics based scrolling
Plug 'yuttie/comfortable-motion.vim'

" Status bar
Plug 'vim-airline/vim-airline'

" For linters and fixers
Plug 'dense-analysis/ale'

" Autocomplete code
" --- Just Some Notes ---
" :PlugClean :PlugInstall :UpdateRemotePlugins
"
" :CocInstall coc-python
" :CocInstall coc-clangd
" :CocInstall coc-snippets
" :CocCommand snippets.edit... FOR EACH FILE TYPE
Plug 'neoclide/coc.nvim'

" Development icons
Plug 'ryanoasis/vim-devicons'

" CTRL + N for multiple cursors
Plug 'terryma/vim-multiple-cursors'

" Vim Terminal
Plug 'tc50cal/vim-terminal'

" Tagbar for code navigation
Plug 'preservim/tagbar'

call plug#end()

"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" VIM SETTINGS
"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
set ruler

" Show relative line numbers
set number relativenumber

" Show current command
set showcmd

" Show current mode
set showmode

" Enable autocompletion for commands
set wildmenu
set wildmode=list:longest,full

" Autocomplete words
set complete+=kspell
set completeopt=menuone,longest

" Search settings
" Searches are case insensitive
set ignorecase
" Scroll to search
set incsearch
" if search term is lowercase, case insensitive search is used, else case
" sensitive search is used
set smartcase
" Highlight search
set hlsearch
" Toggle Highlight
nnoremap <Leader>h :nohl<CR>

" Wrap text
set wrap
" Do not break wrap in the middle of words
set lbr

" Indentation
set autoindent
set smartindent

" Set color scheme
set bg=dark
syntax enable
colorscheme monokai
set termguicolors

set history=1000

set scrolloff=10

set tabstop=4

" Splits open at the bottom and right
set splitbelow splitright

set encoding=utf-8

"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" CUSTOM SHORTCUTS
"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" Window Navigation
nnoremap <C-h> <C-w>h
nnoremap <C-j> <C-w>j
nnoremap <C-k> <C-w>k
nnoremap <C-l> <C-w>l

" Remap ESC to ii
:imap ii <Esc>

" Make adjusing split sizes a bit more friendly
noremap <silent> <C-Left> :vertical resize +3<CR>
noremap <silent> <C-Right> :vertical resize -3<CR>
noremap <silent> <C-Up> :resize +3<CR>
noremap <silent> <C-Down> :resize -3<CR>

" Change 2 split windows from vert to horiz or horiz to vert
map <Leader>th <C-w>t<C-w>H
map <Leader>tk <C-w>t<C-w>K

" Open Xonsh shell
map <Leader>tt :new term://~/anaconda3/envs/xonsh/bin/xonsh --rc ~/.xonshrc<CR>

" Enable clipboard
set clipboard+=unnamedplus

" spelling languages
set spelllang=en,cjk
" Enable the line below to turn on spell checking by default
set spell
nnoremap <silent> <F11> :set spell!<cr>
inoremap <silent> <F11> <C-O>:set spell!<cr>

" Move selection
vnoremap J :m '>+1<CR>gv=gv
vnoremap K :m '<-2<CR>gv=gv

" Open registers
nnoremap <Leader>c :reg<CR>

"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" PLUGIN SETTINGS
"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" Highlight Colors
let g:Hexokinase_highlighters = ['backgroundfull']

" FZF shortcuts, ! opens in full screen
nnoremap <Leader>ff :Files!<CR>
nnoremap <Leader>fl :Lines!<CR>
nnoremap <Leader>f/ :BLines!<CR>
nnoremap <Leader>fm :Marks!<CR>
nnoremap <Leader>fw :Windows!<CR>
nnoremap <Leader>fr :Rg!<CR>
nnoremap <Leader>fc :Colors!<CR>
nnoremap <Leader>fh :History!<CR>
nnoremap <Leader>fx :Commands!<CR>
nnoremap <Leader>fb :Buffers!<CR>

" Easymotion
" <Leader>f{char} to move to {char}
map  <Leader>gf <Plug>(easymotion-bd-f)
nmap <Leader>gf <Plug>(easymotion-overwin-f)

" s{char}{char} to move to {char}{char}
nmap <Leader>gs <Plug>(easymotion-overwin-f2)

" Move to line
map <Leader>gl <Plug>(easymotion-bd-jk)
nmap <Leader>gl <Plug>(easymotion-overwin-line)

" Move to word
map  <Leader>gw <Plug>(easymotion-bd-w)
nmap <Leader>gw <Plug>(easymotion-overwin-w)

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
noremap <silent><expr> ?  incsearch#go(<SID>incsearch_config({'command': '?'}))
noremap <silent><expr> g/ incsearch#go(<SID>incsearch_config({'is_stay': 1}))

" Replace With Register
nmap <Leader>r  <Plug>ReplaceWithRegisterOperator
nmap <Leader>rr <Plug>ReplaceWithRegisterLine
xmap <Leader>r  <Plug>ReplaceWithRegisterVisual

" Comfortable motion
let g:comfortable_motion_scroll_down_key = "j"
let g:comfortable_motion_scroll_up_key = "k"

" Neovide
" set guifont=SauceCodePro\ Nerd\ Font\ Mono:h18
set guifont=LM\ Mono\ 10:h20
let g:neovide_transparency=0.95
let g:neovide_cursor_antialiasing=v:true
" let g:neovide_cursor_vfx_mode = "pixiedust"

" ALE
let g:ale_linters={'python' : ['pylint']}
let g:ale_fixers={'*' : ['remove_trailing_lines', 'trim_whitespace'], 'python' : ['black', 'isort']}
let g:ale_fix_on_save=1

" Nerdtree
nnoremap <A-f> :NERDTreeFocus<CR>
" nnoremap <A-n> :NERDTree<CR>
nnoremap <A-t> :NERDTreeToggle<CR>
nnoremap <A-l> :call CocActionAsync('jumpDefinition')<CR>

" Tagbar
nmap <F8> :TagbarToggle<CR>

" Coc
inoremap <expr> <Tab> pumvisible() ? coc#_select_confirm() : "<Tab>"

"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" CUSTOM MACROS
"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" Add incrementing numbers
let @i ='let i = 1 | g/^/s/^/\=printf("%03d ", i)/ | let i = i+1'

" Remove NA - from lines
let @r ='ggG4lx'
