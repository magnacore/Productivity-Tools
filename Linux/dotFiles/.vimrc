call plug#begin('~/.vim/plugged')

Plug 'junegunn/fzf', { 'do': { -> fzf#install() } }
Plug 'junegunn/fzf.vim'

Plug 'easymotion/vim-easymotion'
Plug 'haya14busa/incsearch.vim'
Plug 'haya14busa/incsearch-fuzzy.vim'
Plug 'haya14busa/incsearch-easymotion.vim'

Plug 'tpope/vim-surround'

Plug 'inkarkat/vim-ReplaceWithRegister'

Plug 'preservim/nerdtree'

Plug 'tpope/vim-commentary'

Plug 'rrethy/vim-hexokinase', { 'do': 'make hexokinase' }

Plug 'christoomey/vim-titlecase'

Plug 'morhetz/gruvbox'
Plug 'sickill/vim-monokai'

Plug 'yuttie/comfortable-motion.vim'

call plug#end()

set ruler

set number relativenumber

set showcmd

set wildmenu

"set hlsearch

"set incsearch

"set smartcase

set lbr

set autoindent

set smartindent

" Set color scheme
set bg=dark
syntax enable
colorscheme monokai
set termguicolors
"let &t_8f = "\<Esc>[38;2;%lu;%lu;%lum"
"let &t_8b = "\<Esc>[48;2;%lu;%lu;%lum"

set wrap

set history=1000

set scrolloff=5

set tabstop=4

" Add incrementing numbers
let @i ='let i = 1 | g/^/s/^/\=printf("%03d ", i)/ | let i = i+1'

" Remove NA - from lines
let @r ='ggG4lx' 

" Highlight Colors
let g:Hexokinase_highlighters = ['backgroundfull']

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

" Toggle Highlight
nnoremap <Leader>h :nohl<CR> 

" Open registers
nnoremap <Leader>r :reg<CR>

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

noremap <silent><expr> /  incsearch#go(<SID>config_easyfuzzymotion())
noremap <silent><expr> ?  incsearch#go(<SID>incsearch_config({'command': '?'}))
noremap <silent><expr> g/ incsearch#go(<SID>incsearch_config({'is_stay': 1}))
noremap <silent><expr> <Space>/ incsearch#go(<SID>incsearch_config())

" Replace With Register
nmap <Leader>p  <Plug>ReplaceWithRegisterOperator
nmap <Leader>pp <Plug>ReplaceWithRegisterLine
xmap <Leader>p  <Plug>ReplaceWithRegisterVisual

" Enable autocompletion:
set wildmode=longest,list,full

" Splits open at the bottom and right
set splitbelow splitright

" Comfortable motion
let g:comfortable_motion_scroll_down_key = "j"
let g:comfortable_motion_scroll_up_key = "k"

" Neovide
" set guifont=SauceCodePro\ Nerd\ Font\ Mono:h18
set guifont=LM\ Mono\ 10:h20
let g:neovide_transparency=0.95
let g:neovide_cursor_antialiasing=v:true
" let g:neovide_cursor_vfx_mode = "pixiedust"
