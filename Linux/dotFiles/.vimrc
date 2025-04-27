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
Plug 'dense-analysis/ale'

" Autocomplete code
" :CocCommand snippets.edit... FOR EACH FILE TYPE
" Plug 'neoclide/coc.nvim'

" Development icons
Plug 'ryanoasis/vim-devicons'

" CTRL + N for multiple cursors
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

call plug#end()

"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" VIM SETTINGS
"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""

" Default Leader is used which is \

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
" Light and dark themes will switch to dark mode
set bg=dark
syntax enable
colorscheme monokai_pro
"colorscheme NeoSolarized
let g:airline_theme='molokai'
"let g:airline_theme='solarized'
"let g:airline_theme='base16_monokai'

if (has("termguicolors"))
		set termguicolors
endif

" Make the background transparent
highlight Normal guibg=none
highlight NonText guibg=none
highlight Normal ctermbg=none
highlight NonText ctermbg=none

set history=1000

set scrolloff=5

set tabstop=4

" Splits open at the bottom and right
set splitbelow splitright

set encoding=utf-8
set fileformat=unix

set title

"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" CUSTOM SHORTCUTS
"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" Window Navigation
nnoremap <C-h> <C-w>h
nnoremap <C-j> <C-w>j
nnoremap <C-k> <C-w>k
nnoremap <C-l> <C-w>l

" Remap ESC to ii
" :imap ii <Esc>

" Make adjusing split sizes a bit more friendly
noremap <C-Left> :vertical resize +3<CR>
noremap <C-Right> :vertical resize -3<CR>
noremap <C-Up> :resize +3<CR>
noremap <C-Down> :resize -3<CR>

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
nnoremap <silent> <F10> :set spell!<cr>
inoremap <silent> <F10> <C-O>:set spell!<cr>
inoremap <C-s> <c-g>u<Esc>[s1z=`]a<c-g>u

" Move selection
vnoremap <A-j> :m '>+1<CR>gv=gv
vnoremap <A-k> :m '<-2<CR>gv=gv

" Open registers
nnoremap <Leader>c :reg<CR>

"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" PLUGIN SETTINGS
"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" Highlight Colors
" let g:Hexokinase_highlighters = ['backgroundfull']

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
" let g:comfortable_motion_scroll_down_key = "j"
" let g:comfortable_motion_scroll_up_key = "k"

" Neovide
" set guifont=SauceCodePro\ Nerd\ Font\ Mono:h18
" set guifont=LM\ Mono\ 10:h20
" let g:neovide_transparency=0.95
" let g:neovide_cursor_antialiasing=v:true
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

let g:NERDTreeGitStatusWithFlags = 1
let g:NERDTreeIgnore = ['^node_modules$']

" " Highlight currently open buffer in NERDTree
" autocmd BufEnter * call SyncTree()
" " sync open file with NERDTree
" " Check if NERDTree is open or active
" function! IsNERDTreeOpen()
"   return exists("t:NERDTreeBufName") && (bufwinnr(t:NERDTreeBufName) != -1)
" endfunction

" " Call NERDTreeFind iff NERDTree is active, current window contains a modifiable
" " file, and we're not in vimdiff
" function! SyncTree()
"   if &modifiable && IsNERDTreeOpen() && strlen(expand('%')) > 0 && !&diff
"     NERDTreeFind
"     wincmd p
"   endif
" endfunction

" Tagbar
nmap <F8> :TagbarToggle<CR>

" Coc
" inoremap <expr> <Tab> pumvisible() ? coc#_select_confirm() : "<Tab>"
"
" set cmdheight=2
"
" " Having longer updatetime (default is 4000 ms = 4 s) leads to noticeable
" " delays and poor user experience.
" " set updatetime=300
"
" " TextEdit might fail if hidden is not set.
" set hidden
"
" " Some servers have issues with backup files, see #649.
" set nobackup
" set nowritebackup
"
" " Don't pass messages to |ins-completion-menu|.
" set shortmess+=c
"
" let g:coc_global_extensions = ['coc-snippets', 'coc-pairs',  'coc-python']
"
" " Use `[g` and `]g` to navigate diagnostics
" nmap <silent> [g <Plug>(coc-diagnostic-prev)
" nmap <silent> ]g <Plug>(coc-diagnostic-next)
"
" " Remap keys for gotos
" nmap <silent> gd <Plug>(coc-definition)
" nmap <silent> gy <Plug>(coc-type-definition)
" nmap <silent> gi <Plug>(coc-implementation)
" nmap <silent> gr <Plug>(coc-references)
"
" " Use K to show documentation in preview window.
" nnoremap <silent> K :call <SID>show_documentation()<CR>
"
" function! s:show_documentation()
"   if (index(['vim','help'], &filetype) >= 0)
"     execute 'h '.expand('<cword>')
"   elseif (coc#rpc#ready())
"     call CocActionAsync('doHover')
"   else
"     execute '!' . &keywordprg . " " . expand('<cword>')
"   endif
" endfunction
"
" " Highlight symbol under cursor on CursorHold
" autocmd CursorHold * silent call CocActionAsync('highlight')
"
" " Remap for rename current word
" nmap <F2> <Plug>(coc-rename)
"
" " Always show the signcolumn, otherwise it would shift the text each time
" " diagnostics appear/become resolved.
" if has("nvim-0.5.0") || has("patch-8.1.1564")
"   " Recently vim can merge signcolumn and number column into one
"   set signcolumn=number
" else
"   set signcolumn=yes
" endif
"
" " Use tab for trigger completion with characters ahead and navigate.
" " NOTE: Use command ':verbose imap <tab>' to make sure tab is not mapped by
" " other plugin before putting this into your config.
" inoremap <silent><expr> <TAB>
"       \ pumvisible() ? "\<C-n>" :
"       \ <SID>check_back_space() ? "\<TAB>" :
"       \ coc#refresh()
" inoremap <expr><S-TAB> pumvisible() ? "\<C-p>" : "\<C-h>"
"
" function! s:check_back_space() abort
"   let col = col('.') - 1
"   return !col || getline('.')[col - 1]  =~# '\s'
" endfunction
"
" " Use <c-space> to trigger completion.
" if has('nvim')
"   inoremap <silent><expr> <c-space> coc#refresh()
" else
"   inoremap <silent><expr> <c-@> coc#refresh()
" endif
"
" " Make <CR> auto-select the first completion item and notify coc.nvim to
" " format on enter, <cr> could be remapped by other vim plugin
" inoremap <silent><expr> <cr> pumvisible() ? coc#_select_confirm()
"                               \: "\<C-g>u\<CR>\<c-r>=coc#on_enter()\<CR>"
"
" " Remap for format selected region
" xmap <leader>cf  <Plug>(coc-format-selected)
" nmap <leader>cf  <Plug>(coc-format-selected)
"
" " Remap for do codeAction of selected region, ex: `<leader>caap` for current paragraph
" xmap <leader>ca  <Plug>(coc-codeaction-selected)
" nmap <leader>ca  <Plug>(coc-codeaction-selected)
"
" augroup mygroup
"   autocmd!
"   " Setup formatexpr specified filetype(s).
"   autocmd FileType typescript,json setl formatexpr=CocAction('formatSelected')
"   " Update signature help on jump placeholder.
"   autocmd User CocJumpPlaceholder call CocActionAsync('showSignatureHelp')
" augroup end
"
" " Run the Code Lens action on the current line.
" nmap <leader>cl  <Plug>(coc-codelens-action)
"
" " Remap for do codeAction of current line
" nmap <leader>cac  <Plug>(coc-codeaction)
"
" " Fix autofix problem of current line
" nmap <leader>cqf  <Plug>(coc-fix-current)
"
" " Remap <C-f> and <C-b> for scroll float windows/popups.
" if has('nvim-0.4.0') || has('patch-8.2.0750')
"   nnoremap <silent><nowait><expr> <C-f> coc#float#has_scroll() ? coc#float#scroll(1) : "\<C-f>"
"   nnoremap <silent><nowait><expr> <C-b> coc#float#has_scroll() ? coc#float#scroll(0) : "\<C-b>"
"   inoremap <silent><nowait><expr> <C-f> coc#float#has_scroll() ? "\<c-r>=coc#float#scroll(1)\<cr>" : "\<Right>"
"   inoremap <silent><nowait><expr> <C-b> coc#float#has_scroll() ? "\<c-r>=coc#float#scroll(0)\<cr>" : "\<Left>"
"   vnoremap <silent><nowait><expr> <C-f> coc#float#has_scroll() ? coc#float#scroll(1) : "\<C-f>"
"   vnoremap <silent><nowait><expr> <C-b> coc#float#has_scroll() ? coc#float#scroll(0) : "\<C-b>"
" endif

" if $CONDA_PREFIX == ""
"   let s:current_python_path=$CONDA_PYTHON_EXE
" else
"   let s:current_python_path=$CONDA_PREFIX.'/bin/python'
" endif
" call coc#config('python', {'pythonPath': s:current_python_path})

" Vim bufftagline
nnoremap <Leader>bn :bnext<CR>
nnoremap <Leader>bp :bprev<CR>

"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" CUSTOM MACROS
"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" Add incrementing numbers
let @i ='let i = 1 | g/^/s/^/\=printf("%03d ", i)/ | let i = i+1'

" Remove NA - from lines
let @r ='ggG4lx'


"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
" Abbreviations
"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
ab mcs Manuj Chandra Sharma
ab gai Generative AI
ab config configuration
