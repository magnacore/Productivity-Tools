call plug#begin('~/.vim/plugged')

Plug 'junegunn/fzf', { 'do': { -> fzf#install() } }
Plug 'junegunn/fzf.vim'

Plug 'easymotion/vim-easymotion'
Plug 'haya14busa/incsearch.vim'
Plug 'haya14busa/incsearch-fuzzy.vim'
Plug 'haya14busa/incsearch-easymotion.vim'

Plug 'tpope/vim-surround'

Plug 'inkarkat/vim-ReplaceWithRegister'

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

set bg=dark

set wrap

set history=1000

set scrolloff=5

set tabstop=4

" add incrementing numbers
let @i ='let i = 1 | g/^/s/^/\=printf("%03d ", i)/ | let i = i+1'

" remove NA - from lines
let @r ='ggG4lx' 

" enable clipboard
set clipboard+=unnamedplus

" spelling languages
set spelllang=en,cjk
" Enable the line below to turn on spell checking by default
set spell
nnoremap <silent> <F11> :set spell!<cr>
inoremap <silent> <F11> <C-O>:set spell!<cr>

vnoremap J :m '>+1<CR>gv=gv 
vnoremap K :m '<-2<CR>gv=gv 

nnoremap <Leader>h :nohl<CR> 

" FZF shortcuts, ! opens in full screen
nnoremap <Leader>ff :Files!<CR>
nnoremap <Leader>fl :Lines!<CR>
nnoremap <Leader>fb :BLines!<CR>
nnoremap <Leader>fm :Marks!<CR>
nnoremap <Leader>fw :Windows!<CR>
nnoremap <Leader>fr :Rg!<CR>

"Easymotion

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

nmap <Leader>r  <Plug>ReplaceWithRegisterOperator
nmap <Leader>rr <Plug>ReplaceWithRegisterLine
xmap <Leader>r  <Plug>ReplaceWithRegisterVisual

" Neovide
" set guifont=SauceCodePro\ Nerd\ Font\ Mono:h18
set guifont=LM\ Mono\ 10:h20
let g:neovide_transparency=0.95
let g:neovide_cursor_antialiasing=v:true
" let g:neovide_cursor_vfx_mode = "pixiedust"
