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

" Neovide
" set guifont=SauceCodePro\ Nerd\ Font\ Mono:h18
set guifont=LM\ Mono\ 10:h20
let g:neovide_transparency=0.95
let g:neovide_cursor_antialiasing=v:true
" let g:neovide_cursor_vfx_mode = "pixiedust"
