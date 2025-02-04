# This is a sample commands.py.  You can add your own commands here.
#
# Please refer to commands_full.py for all the default commands and a complete
# documentation.  Do NOT add them all here, or you may end up with defunct
# commands when upgrading ranger.

# A simple command for demonstration purposes follows.
# -----------------------------------------------------------------------------

# You can import any python module as needed.
import os
from collections import deque

# You always need to import ranger.api.commands here to get the Command class:
from ranger.api.commands import Command
from ranger_udisk_menu.mounter import mount


# # Any class that is a subclass of "Command" will be integrated into ranger as a
# # command.  Try typing ":my_edit<ENTER>" in ranger!
# class my_edit(Command):
#     # The so-called doc-string of the class will be visible in the built-in
#     # help that is accessible by typing "?c" inside ranger.
#     """:my_edit <filename>

#     A sample command for demonstration purposes that opens a file in an editor.
#     """

#     # The execute method is called when you run this command in ranger.
#     def execute(self):
#         # self.arg(1) is the first (space-separated) argument to the function.
#         # This way you can write ":my_edit somefilename<ENTER>".
#         if self.arg(1):
#             # self.rest(1) contains self.arg(1) and everything that follows
#             target_filename = self.rest(1)
#         else:
#             # self.fm is a ranger.core.filemanager.FileManager object and gives
#             # you access to internals of ranger.
#             # self.fm.thisfile is a ranger.container.file.File object and is a
#             # reference to the currently selected file.
#             target_filename = self.fm.thisfile.path

#         # This is a generic function to print text in ranger.
#         self.fm.notify("Let's edit the file " + target_filename + "!")

#         # Using bad=True in fm.notify allows you to print error messages:
#         if not os.path.exists(target_filename):
#             self.fm.notify("The given file does not exist!", bad=True)
#             return

#         # This executes a function from ranger.core.acitons, a module with a
#         # variety of subroutines that can help you construct commands.
#         # Check out the source, or run "pydoc ranger.core.actions" for a list.
#         self.fm.edit_file(target_filename)

#     # The tab method is called when you press tab, and should return a list of
#     # suggestions that the user will tab through.
#     # tabnum is 1 for <TAB> and -1 for <S-TAB> by default
#     def tab(self, tabnum):
#         # This is a generic tab-completion function that iterates through the
#         # content of the current directory.
#         return self._tab_directory_content()


###############################################################################


class fzf_select(Command):
    """
    :fzf_select
    Find a file using fzf.
    With a prefix argument to select only directories.

    See: https://github.com/junegunn/fzf
    """

    def execute(self):
        import subprocess

        from ranger.ext.get_executables import get_executables

        if "fzf" not in get_executables():
            self.fm.notify("Could not find fzf in the PATH.", bad=True)
            return

        fd = None
        if "fdfind" in get_executables():
            fd = "fdfind"
        elif "fd" in get_executables():
            fd = "fd"

        if fd is not None:
            hidden = "--hidden" if self.fm.settings.show_hidden else ""
            exclude = "--no-ignore-vcs --exclude '.git' --exclude '*.py[co]' --exclude '__pycache__'"
            only_directories = "--type directory" if self.quantifier else ""
            fzf_default_command = "{} --follow {} {} {} --color=always".format(
                fd, hidden, exclude, only_directories
            )
        else:
            hidden = (
                "-false" if self.fm.settings.show_hidden else r"-path '*/\.*' -prune"
            )
            exclude = r"\( -name '\.git' -o -iname '\.*py[co]' -o -fstype 'dev' -o -fstype 'proc' \) -prune"
            only_directories = "-type d" if self.quantifier else ""
            fzf_default_command = (
                "find -L . -mindepth 1 {} -o {} -o {} -print | cut -b3-".format(
                    hidden, exclude, only_directories
                )
            )

        env = os.environ.copy()
        env["FZF_DEFAULT_COMMAND"] = fzf_default_command
        env[
            "FZF_DEFAULT_OPTS"
        ] = "--height=100% --layout=reverse --ansi --preview='pistol {}'"

        fzf = self.fm.execute_command(
            "fzf --no-multi", env=env, universal_newlines=True, stdout=subprocess.PIPE
        )
        stdout, _ = fzf.communicate()
        if fzf.returncode == 0:
            selected = os.path.abspath(stdout.strip())
            if os.path.isdir(selected):
                self.fm.cd(selected)
            else:
                self.fm.select_file(selected)


# fzf_locate
class fzf_locate(Command):
    """
    :fzf_locate

    Find a file using fzf.

    With a prefix argument select only directories.

    See: https://github.com/junegunn/fzf
    """

    def execute(self):
        import subprocess

        if self.quantifier:
            # command = "locate home media | fzf -e -i" # use this command for mlocate
            command = "locate home | fzf -e -i" # for plocate
        else:
            # command = "locate home media | fzf -e -i" # use this command for mlocate
            command = "locate home | fzf -e -i" # for plocate
        fzf = self.fm.execute_command(command, stdout=subprocess.PIPE)
        stdout, stderr = fzf.communicate()
        if fzf.returncode == 0:
            fzf_file = os.path.abspath(stdout.decode("utf-8").rstrip("\n"))
            if os.path.isdir(fzf_file):
                self.fm.cd(fzf_file)
            else:
                self.fm.select_file(fzf_file)


###############################################################################


class fd_search(Command):
    """
    :fd_search [-d<depth>] <query>
    Executes "fd -d<depth> <query>" in the current directory and focuses the
    first match. <depth> defaults to 1, i.e. only the contents of the current
    directory.

    See https://github.com/sharkdp/fd
    """

    SEARCH_RESULTS = deque()

    def execute(self):
        import re
        import subprocess

        from ranger.ext.get_executables import get_executables

        self.SEARCH_RESULTS.clear()

        if "fdfind" in get_executables():
            fd = "fdfind"
        elif "fd" in get_executables():
            fd = "fd"
        else:
            self.fm.notify("Couldn't find fd in the PATH.", bad=True)
            return

        if self.arg(1):
            if self.arg(1)[:2] == "-d":
                depth = self.arg(1)
                target = self.rest(2)
            else:
                depth = "-d1"
                target = self.rest(1)
        else:
            self.fm.notify(":fd_search needs a query.", bad=True)
            return

        hidden = "--hidden" if self.fm.settings.show_hidden else ""
        exclude = "--no-ignore-vcs --exclude '.git' --exclude '*.py[co]' --exclude '__pycache__'"
        command = "{} --follow {} {} {} --print0 {}".format(
            fd, depth, hidden, exclude, target
        )
        fd = self.fm.execute_command(
            command, universal_newlines=True, stdout=subprocess.PIPE
        )
        stdout, _ = fd.communicate()

        if fd.returncode == 0:
            results = filter(None, stdout.split("\0"))
            if not self.fm.settings.show_hidden and self.fm.settings.hidden_filter:
                hidden_filter = re.compile(self.fm.settings.hidden_filter)
                results = filter(
                    lambda res: not hidden_filter.search(os.path.basename(res)), results
                )
            results = map(
                lambda res: os.path.abspath(os.path.join(self.fm.thisdir.path, res)),
                results,
            )
            self.SEARCH_RESULTS.extend(sorted(results, key=str.lower))
            if len(self.SEARCH_RESULTS) > 0:
                self.fm.notify(
                    "Found {} result{}.".format(
                        len(self.SEARCH_RESULTS),
                        ("s" if len(self.SEARCH_RESULTS) > 1 else ""),
                    )
                )
                self.fm.select_file(self.SEARCH_RESULTS[0])
            else:
                self.fm.notify("No results found.")


class fd_next(Command):
    """
    :fd_next
    Selects the next match from the last :fd_search.
    """

    def execute(self):
        if len(fd_search.SEARCH_RESULTS) > 1:
            fd_search.SEARCH_RESULTS.rotate(-1)  # rotate left
            self.fm.select_file(fd_search.SEARCH_RESULTS[0])
        elif len(fd_search.SEARCH_RESULTS) == 1:
            self.fm.select_file(fd_search.SEARCH_RESULTS[0])


class fd_prev(Command):
    """
    :fd_prev
    Selects the next match from the last :fd_search.
    """

    def execute(self):
        if len(fd_search.SEARCH_RESULTS) > 1:
            fd_search.SEARCH_RESULTS.rotate(1)  # rotate right
            self.fm.select_file(fd_search.SEARCH_RESULTS[0])
        elif len(fd_search.SEARCH_RESULTS) == 1:
            self.fm.select_file(fd_search.SEARCH_RESULTS[0])


###############################################################################


class mkdirmv(Command):
    """:mkdirmv <target_directory>"""

    def make_safe_path(self, dst):
        if not os.path.exists(dst):
            return dst

        dst_name, dst_ext = os.path.splitext(dst)

        if not dst_name.endswith("_"):
            dst_name += "_"
            if not os.path.exists(dst_name + dst_ext):
                return dst_name + dst_ext
        n = 0
        test_dst = dst_name + str(n)
        while os.path.exists(test_dst + dst_ext):
            n += 1
            test_dst = dst_name + str(n)

        return test_dst + dst_ext

    def execute(self):
        cwd = self.fm.thisdir
        cf = self.fm.thisfile
        if not cwd or not cf:
            self.fm.notify("Error: no file(s) selected", bad=True)
            return
        files = [f for f in self.fm.thistab.get_selection()]

        target_dir = self.rest(1)
        if not target_dir:
            self.fm.notify("Error: target directory not specified", bad=True)
            return

        from os import makedirs
        from os.path import expanduser, join, lexists

        target_dir = join(self.fm.thisdir.path, expanduser(target_dir))

        if not lexists(target_dir):
            makedirs(target_dir)

        for f in files:
            destination = self.make_safe_path(join(target_dir, f.relative_path))
            self.fm.rename(f, destination)

        # Change mode to normal in case visual selection mode was on
        self.fm.change_mode("normal")

        self.fm.notify(f"Done moving to {target_dir}")

    def tab(self):
        return self._tab_directory_content()


###############################################################################


class ranger_pycopy(Command):
    """
    :ranger_pycopy

    Copy selected files to the current directory
    """

    def execute(self):
        # We are passing current directory as the second parameter
        # In the Xonsh script it will be the last item in the list
        self.fm.execute_console(f"shell file-copy-ranger %c %d")

###############################################################################

class ranger_pymove(Command):
    """
    :ranger_pymove

    Move selected files to the current directory
    """

    def execute(self):
        # We are passing current directory as the second parameter
        # In the Xonsh script it will be the last item in the list
        self.fm.execute_console(f"shell file-move-ranger %c %d")

###############################################################################

class toggle_flat(Command):
    """
    :toggle_flat

    Flattens or unflattens the directory view.
    """

    def execute(self):
        if self.fm.thisdir.flat == 0:
            self.fm.thisdir.unload()
            self.fm.thisdir.flat = -1
            self.fm.thisdir.load_content()
            self.fm.notify("Un-flattened.")
        else:
            self.fm.thisdir.unload()
            self.fm.thisdir.flat = 0
            self.fm.thisdir.load_content()
            self.fm.notify("Flattened.")


###############################################################################


class copy_selected_to_highlight(Command):
    """:copy_selected_to_highlight"""

    def execute(self):
        # Get highlighted directory
        target_dir = self.fm.thisfile.relative_path

        if not target_dir:
            self.fm.notify("Error: target directory not highlighted", bad=True)
            return

        from os.path import expanduser, join

        target_dir = join(self.fm.thisdir.path, expanduser(target_dir))

        self.fm.execute_console("copy")
        self.fm.do_cut = False
        self.fm.paste(dest=target_dir)
        self.fm.notify(f"Done copying to {target_dir}")


###############################################################################


class directories_number_highlight(Command):
    """:directories_number_highlight"""

    def execute(self):
        # Get highlighted directory
        target_dir = self.fm.thisfile.relative_path

        if not target_dir:
            self.fm.notify("Error: target directory not highlighted", bad=True)
            return

        from os.path import expanduser, join

        target_dir = join(self.fm.thisdir.path, expanduser(target_dir))

        self.fm.execute_console(f"shell -f directory-number '{target_dir}'")
        self.fm.notify("Done numbering directories.")


###############################################################################


class mark_tag(Command):
    """:mark_tag [<tags>]

    Mark all tags that are tagged with either of the given tags.
    When leaving out the tag argument, all tagged files are marked.
    """

    do_mark = True

    def execute(self):
        cwd = self.fm.thisdir
        tags = self.rest(1).replace(" ", "")
        if not self.fm.tags or not cwd.files:
            return
        for fileobj in cwd.files:
            try:
                tag = self.fm.tags.tags[fileobj.realpath]
            except KeyError:
                continue
            if not tags or tag in tags:
                cwd.mark_item(fileobj, val=self.do_mark)
        self.fm.ui.status.need_redraw = True
        self.fm.ui.need_redraw = True


class unmark_tag(mark_tag):
    """:unmark_tag [<tags>]

    Unmark all tags that are tagged with either of the given tags.
    When leaving out the tag argument, all tagged files are unmarked.
    """

    do_mark = False


###############################################################################


class image_convert(Command):
    """:Resize images"""

    def execute(self):
        # self.arg(1) is the first (space-separated) argument to the function.
        # This way you can write ":my_edit somefilename<ENTER>".
        if self.arg(1):
            # self.rest(1) contains self.arg(1) and everything that follows
            dimension = self.rest(1)
        else:
            dimension = 1080

        # %s sends each file as an argument
        self.fm.execute_console(f"shell image-convert {dimension} %s")
        self.fm.change_mode("normal")


###############################################################################


class open_in_tabs(Command):
    """
    :open_in_tabs
    Open one highlighted or several selected folders in new tab
    If only one folder is highlighted, it will be treated as a single selection
    """

    def execute(self):
        cwd = self.fm.thisdir
        cf = self.fm.thisfile
        if not cwd or not cf:
            self.fm.notify("Error: no file(s) selected", bad=True)
            return

        files = [f for f in self.fm.thistab.get_selection()]

        for f in files:
            # narg=f.relative_path sets the name of the tab
            self.fm.tab_new(narg=f.relative_path, path=f"{f}")

        self.fm.change_mode("normal")


###############################################################################


class gpg_detached_sign(Command):
    """
    :gpg_detached_sign
    Creates detached signatures for a file using gpg
    If only one file is highlighted, it will be treated as a single selection
    """

    def execute(self):
        cwd = self.fm.thisdir
        cf = self.fm.thisfile
        if not cwd or not cf:
            self.fm.notify("Error: no file(s) selected", bad=True)
            return

        files = [f for f in self.fm.thistab.get_selection()]

        for f in files:
            self.fm.execute_console(
                f"""shell -f gpg --detach-sign -u 80513A7E8F48186D "{f.relative_path}" """
            )

        self.fm.change_mode("normal")
        self.fm.notify("Done signing.")


###############################################################################


class gpg_signature_verify(Command):
    """
    :gpg_signature_verify
    Verifies selected signatures
    If only one file is highlighted, it will be treated as a single selection
    """

    def execute(self):
        cwd = self.fm.thisdir
        cf = self.fm.thisfile
        if not cwd or not cf:
            self.fm.notify("Error: no file(s) selected", bad=True)
            return

        files = [f for f in self.fm.thistab.get_selection()]

        for f in files:
            root_ext = os.path.splitext(f.relative_path)
            if root_ext[1] == ".sig":
                self.fm.execute_console(
                    f"""shell -w gpg --verify "{f.relative_path}" """
                )

        self.fm.change_mode("normal")


###############################################################################


class gpg_encrypt_file(Command):
    """
    :gpg_encrypt_file
    Encrypts a file using gpg
    If only one file is highlighted, it will be treated as a single selection
    """

    def execute(self):
        cwd = self.fm.thisdir
        cf = self.fm.thisfile
        if not cwd or not cf:
            self.fm.notify("Error: no file(s) selected", bad=True)
            return

        files = [f for f in self.fm.thistab.get_selection()]

        for f in files:
            self.fm.execute_console(
                f"""shell -f gpg -e -u F19C46E0E40C9568 -r F19C46E0E40C9568 "{f.relative_path}" """
            )

        self.fm.change_mode("normal")


###############################################################################


class gpg_decrypt_file(Command):
    """
    :gpg_decrypt_file
    Decrypts a file using gpg
    If only one file is highlighted, it will be treated as a single selection
    """

    def execute(self):
        cwd = self.fm.thisdir
        cf = self.fm.thisfile
        if not cwd or not cf:
            self.fm.notify("Error: no file(s) selected", bad=True)
            return

        files = [f for f in self.fm.thistab.get_selection()]

        for f in files:
            root_ext = os.path.splitext(f.relative_path)
            self.fm.execute_console(
                f"""shell -f gpg -o "{root_ext[0]}" -d "{f.relative_path}" """
            )

        self.fm.change_mode("normal")


###############################################################################


class files_tag(Command):
    """:tag files"""

    def execute(self):
        # self.arg(1) is the first (space-separated) argument to the function.
        # This way you can write ":my_edit somefilename<ENTER>".
        if self.arg(1):
            # self.rest(1) contains self.arg(1) and everything that follows
            tag = self.rest(1)
        else:
            tag = "noTag"

        # %s sends each file as an argument
        self.fm.execute_console(f"shell file-tag {tag} %s")
        self.fm.change_mode("normal")


###############################################################################


class files_tag_remove(Command):
    """:remove file tags"""

    def execute(self):
        # self.arg(1) is the first (space-separated) argument to the function.
        # This way you can write ":my_edit somefilename<ENTER>".
        if self.arg(1):
            # self.rest(1) contains self.arg(1) and everything that follows
            tag = self.rest(1)
        else:
            tag = "noTag"

        # %s sends each file as an argument
        self.fm.execute_console(f"shell file-tag-remove {tag} %s")
        self.fm.change_mode("normal")


###############################################################################


class file_convert_text(Command):
    """:Converts file/folder names to text files"""

    def execute(self):
        # %s sends each file as an argument
        self.fm.execute_console(f"shell file-convert-text %s")
        self.fm.change_mode("normal")


###############################################################################


class file_number(Command):
    """:number files based on a starting number and padding"""

    def execute(self):
        # self.arg(1) is the first (space-separated) argument to the function.
        # This way you can write ":my_edit somefilename<ENTER>".
        if self.arg(1):
            # self.rest(1) contains self.arg(1) and everything that follows
            starting_number = self.arg(1)
            padding = self.arg(2)
        else:
            starting_number = 1
            padding = 3

        # %s sends each file as an argument
        self.fm.execute_console(f"shell file-number {starting_number} {padding} %s")
        self.fm.change_mode("normal")


###############################################################################


class document_convert(Command):
    """:convert documents using pandoc"""

    def execute(self):
        # self.arg(1) is the first (space-separated) argument to the function.
        # This way you can write ":my_edit somefilename<ENTER>".
        if self.arg(1):
            # self.rest(1) contains self.arg(1) and everything that follows
            extension = self.rest(1)
        else:
            extension = "pdf"

        # %s sends each file as an argument
        self.fm.execute_console(f"shell document-convert {extension} %s")
        self.fm.change_mode("normal")


###############################################################################

class ocr(Command):
    """
    :ocr
    Converts images to text
    If only one file is highlighted, it will be treated as a single selection
    """

    def execute(self):
        cwd = self.fm.thisdir
        cf = self.fm.thisfile
        if not cwd or not cf:
            self.fm.notify("Error: no file(s) selected", bad=True)
            return
        else:
            # %s sends each file as an argument
            self.fm.execute_console(f"shell image-convert-text %s")
            self.fm.change_mode("normal")

###############################################################################

class paste_ext(Command):
    """
    :paste_ext

    Like paste but tries to rename conflicting files so that the
    file extension stays intact (e.g. file_.ext).
    """

    @staticmethod
    def make_safe_path(dst):
        if not os.path.exists(dst):
            return dst

        dst_name, dst_ext = os.path.splitext(dst)

        if not dst_name.endswith("_"):
            dst_name += "_"
            if not os.path.exists(dst_name + dst_ext):
                return dst_name + dst_ext
        n = 0
        test_dst = dst_name + str(n)
        while os.path.exists(test_dst + dst_ext):
            n += 1
            test_dst = dst_name + str(n)

        return test_dst + dst_ext

    def execute(self):
        return self.fm.paste(make_safe_path=paste_ext.make_safe_path)

###############################################################################

class audio_convert_foss(Command):
    """:Convert common audio formats to foss"""

    def execute(self):
        # self.arg(1) is the first (space-separated) argument to the function.
        # This way you can write ":my_edit somefilename<ENTER>".
        if self.arg(1):
            # self.rest(1) contains self.arg(1) and everything that follows
            bitrate = self.rest(1)
        else:
            bitrate = 128

        # %s sends each file as an argument
        self.fm.execute_console(f"shell audio-convert-foss {bitrate} %s")
        self.fm.change_mode("normal")

###############################################################################

class video_convert_audio(Command):
    """:Extract audio from a video without conversion"""

    def execute(self):

        # %s sends each file as an argument
        self.fm.execute_console(f"shell video-convert-audio %s")
        self.fm.change_mode("normal")

###############################################################################

class pdf_split(Command):
    """:PDF Split"""

    def execute(self):
        # self.arg(1) is the first (space-separated) argument to the function.
        # This way you can write ":my_edit somefilename<ENTER>".
        if self.arg(1):
            # self.rest(1) contains self.arg(1) and everything that follows
            split_after = self.rest(1)
        else:
            split_after = 1

        # %s sends each file as an argument
        self.fm.execute_console(f"shell pdf-split {split_after} %s")
        self.fm.change_mode("normal")

###############################################################################

class media_combine(Command):
    """:Combine similar media files"""

    def execute(self):

        # %s sends each file as an argument
        self.fm.execute_console(f"shell media-combine %s")
        self.fm.change_mode("normal")

###############################################################################

class file_rename_valid(Command):
    """:Set valid filenames"""

    def execute(self):

        # %s sends each file as an argument
        self.fm.execute_console(f"shell file-rename-valid %s")
        self.fm.change_mode("normal")
        self.fm.notify("Files renamed.")

###############################################################################

class pdf_combine(Command):
    """:Combine pdf files"""

    def execute(self):

        # %s sends each file as an argument
        self.fm.execute_console(f"shell pdf-combine %s")
        self.fm.change_mode("normal")

###############################################################################

class text_split(Command):
    """:Text Split"""

    def execute(self):
        # self.arg(1) is the first (space-separated) argument to the function.
        # This way you can write ":my_edit somefilename<ENTER>".
        if self.arg(1):
            # self.rest(1) contains self.arg(1) and everything that follows
            split_after = self.rest(1)
        else:
            split_after = 10

        # %s sends each file as an argument
        self.fm.execute_console(f"shell text-split {split_after} %s")
        self.fm.change_mode("normal")

###############################################################################

class image_combine_pdf(Command):
    """:Combine images into pdf files"""

    def execute(self):

        # %s sends each file as an argument
        self.fm.execute_console(f"shell image-combine-pdf %s")
        self.fm.change_mode("normal")

###############################################################################

class text_to_speech(Command):
    """
    :text_to_speech
    Converts text to speech using Google
    If only one file is highlighted, it will be treated as a single selection
    """

    def execute(self):
        cwd = self.fm.thisdir
        cf = self.fm.thisfile
        if not cwd or not cf:
            self.fm.notify("Error: no file(s) selected", bad=True)
            return
        else:
            # %s sends each file as an argument
            self.fm.execute_console(f"shell text-to-speech %s")
            self.fm.change_mode("normal")

###############################################################################

class pdf_convert_text(Command):
    """:Convert PDF to text"""

    def execute(self):
        cwd = self.fm.thisdir
        cf = self.fm.thisfile
        if not cwd or not cf:
            self.fm.notify("Error: no file(s) selected", bad=True)
            return
        else:
            # %s sends each file as an argument
            self.fm.execute_console(f"shell pdf-convert-text %s")
            self.fm.change_mode("normal")

###############################################################################

class mkv_extract_track(Command):
    """:Interactively extract tracks from mkv/a without conversion"""

    def execute(self):

        # %s sends each file as an argument
        self.fm.execute_console(f"shell mkv-extract-track %s")
        self.fm.change_mode("normal")

###############################################################################

class embed_subtitle(Command):
    """:Embed a subtitle in a video file."""

    def execute(self):
        self.fm.execute_console(f"shell embed-subtitle %s")

        self.fm.change_mode("normal")
        self.fm.notify("Subtitle embedded.")

###############################################################################

class epub_convert_multiple_tui(Command):
    """
    :epub_convert_multiple
    Converts epub to text or pdf
    If only one file is highlighted, it will be treated as a single selection
    """

    def execute(self):
        cwd = self.fm.thisdir
        cf = self.fm.thisfile
        if not cwd or not cf:
            self.fm.notify("Error: no file(s) selected", bad=True)
            return
        else:
            # %s sends each file as an argument
            self.fm.execute_console(f"shell epub-convert-multiple-tui %s")
            self.fm.change_mode("normal")

###############################################################################

class files_tag_percentage(Command):
    """:tag files with percentage"""

    def execute(self):
        # %s sends each file as an argument
        self.fm.execute_console(f"shell file-tag-percentage %s")
        self.fm.change_mode("normal")


###############################################################################

class files_tag_remove_percentage(Command):
    """:remove files tags with percentage"""

    def execute(self):
        # %s sends each file as an argument
        self.fm.execute_console(f"shell file-tag-remove-percentage %s")
        self.fm.change_mode("normal")

###############################################################################

# class media_split_equal(Command):
#     """
#     :media_split_equal <split_duration_seconds> <threshold_seconds> <files>
#     Splits media
#     """

#     def execute(self):
#         # self.arg(1) is the first (space-separated) argument to the function.
#         # This way you can write ":my_edit somefilename<ENTER>".
#         if self.arg(1) and self.arg(2):
#             duration = self.arg(1)
#             threshold = self.arg(2)
#         else:
#             duration = 300
#             threshold = 600

#         # %s sends each file as an argument
#         self.fm.execute_console(f"shell media-split-equal {duration} {threshold} %s")

#         self.fm.change_mode("normal")

###############################################################################

class file_number_remove(Command):
    """:remove numbers from the start of filenames"""

    def execute(self):
        # %s sends each file as an argument
        self.fm.execute_console(f"shell file-number-remove %s")
        self.fm.change_mode("normal")

###############################################################################

class files_group_move(Command):
    """:groups files by name and moves them to a folder"""

    def execute(self):
        # %s sends each file as an argument
        self.fm.execute_console(f"shell files-group-move %s")
        self.fm.change_mode("normal")

###############################################################################

class audio_add_music(Command):
    """:Add background music to audio file."""

    def execute(self):
        self.fm.execute_console(f"shell audio-add-music %s")
        self.fm.change_mode("normal")
        self.fm.notify("Background music embedded.")

###############################################################################

class audio_process(Command):
    """
    : Change volume of audio
    : audio_process
    """

    def execute(self):
        # %s sends each file as an argument
        self.fm.execute_console(f"shell audio-process %s")
        self.fm.change_mode("normal")

###############################################################################

class media_length_tag(Command):
    """
    : Calculate length of videos in subdirectores of selected folders
    : with a specific tag
    : Demonstrates how commands can be executed on selected folders
    """

    def execute(self):
        cwd = self.fm.thisdir
        cf = self.fm.thisfile

        if not cwd or not cf:
            self.fm.notify("Error: no file(s) selected", bad=True)
            return

        files = [f for f in self.fm.thistab.get_selection()]

        for f in files:
            self.fm.execute_console(f"""shell -w media-length-tag '{f.relative_path}' """)

        self.fm.change_mode("normal")

###############################################################################

class file_rename_title(Command):
    """:Set filenames to title case"""

    def execute(self):

        # %s sends each file as an argument
        self.fm.execute_console(f"shell file-rename-title %s")
        self.fm.change_mode("normal")
        self.fm.notify("Files renamed.")

###############################################################################

class file_rename_extension(Command):
    """:Change filename extension"""

    def execute(self):

        # %s sends each file as an argument
        self.fm.execute_console(f"shell file-rename-extension %s")
        self.fm.change_mode("normal")
        self.fm.notify("Extension renamed.")

###############################################################################

class image_watermark(Command):
    """:Watermark images"""

    def execute(self):
        cwd = self.fm.thisdir
        cf = self.fm.thisfile
        if not cwd or not cf:
            self.fm.notify("Error: no file(s) selected", bad=True)
            return
        else:
            # %s sends each file as an argument
            self.fm.execute_console(f"shell image-watermark-tui %s")
            self.fm.change_mode("normal")

###############################################################################

# pip install llama-index-llms-ollama

# from llama_index.llms.ollama import Ollama
# from llama_index.core.tools import FunctionTool
# from llama_index.core.agent import ReActAgent
# from llama_index.core import PromptTemplate


# class venom(Command):
#     """:Use Generative AI to call tools"""

#     def execute(self):
#         # self.arg(1) is the first (space-separated) argument to the function.
#         # This way you can write ":my_edit somefilename<ENTER>".
#         if self.arg(1):
#             # self.rest(1) contains self.arg(1) and everything that follows
#             argument = self.rest(1)        

#         llm = Ollama(model="llama3.1", base_url='http://localhost:11434', temperature=0.0, request_timeout=600)

#         react_system_header_str = """\

#         You are designed to help with calling tools based on user input.

#         ## Tools
#         You have access to a wide variety of tools. You are responsible for using
#         the tools in any sequence you deem appropriate to complete the task at hand.
#         This may require breaking the task into subtasks and using different tools
#         to complete each subtask.

#         You have access to the following tools:
#         {tool_desc}

#         ## Output Format
#         To answer the question, please use the following format.

#         ```
#         Thought: I need to use a tool to help me answer the question.
#         Action: tool name (one of {tool_names}) if using a tool.
#         Action Input: the input to the tool, in a JSON format representing the kwargs (e.g. {{"input": "hello world", "num_beams": 5}})
#         ```

#         Please ALWAYS start with a Thought.

#         Please use a valid JSON format for the Action Input. Do NOT do this {{'input': 'hello world', 'num_beams': 5}}.

#         You should keep repeating the above format until you have enough information
#         to answer the question without using any more tools. At that point, you MUST respond
#         in the one of the following two formats:

#         ```
#         Thought: I can answer without using any more tools.
#         Answer: [your answer here]
#         ```

#         ```
#         Thought: I cannot answer the question with the provided tools.
#         Answer: Sorry, I cannot answer your query.
#         ```

#         ## Additional Rules
#         - You MUST obey the function signature of each tool. Do NOT pass in no arguments if the function expects arguments.

#         ## Current Conversation
#         Below is the current conversation consisting of interleaving human and assistant messages.

#         """

#         react_system_prompt = PromptTemplate(react_system_header_str)
        
#         def select_files(pattern:str):
#             """
#             Selects files based on the provided regex pattern.
#             If asked to select mkv files pattern would be mkv.
#             If asked to spect images pattern could be jpg, jpeg, webp, png.
#             The function does not return anything, do not wait for an output.
#             """
#             command = f"scout -m {pattern}"
#             self.fm.execute_console(command)

#         def split_pdf(pages:str):
#             """
#             Split the provided pdf.
#             If a pdf is to be split into 10 pages each, pages would be 10.
#             """
#             command = f"pdf_split {pages}"
#             self.fm.execute_console(command)

#         select_tool = FunctionTool.from_defaults(fn=select_files)
#         split_pdf_tool = FunctionTool.from_defaults(fn=split_pdf)

#         react_agent = ReActAgent.from_tools(tools=[select_tool, split_pdf_tool], llm=llm, verbose=False)
#         react_agent.update_prompts({"agent_worker:system_prompt": react_system_prompt})
#         react_agent.reset()

#         react_agent.chat(argument)
        

###############################################################################

class file_select_similar(Command):
    """:file_select_similar file_name"""

    def execute(self):
        import re
        
        highlighted_file = self.rest(1)
        if not highlighted_file:
            self.fm.notify("Error: No file hilighted", bad=True)
            return

        # remove progress pattern from the file name
        pattern = r'((-part-)?\d{1,4}-\d{1,4}r-\d{1,4}p|\s+#[\w]+)'
        common_filename = re.sub(pattern, '', highlighted_file).strip()
        name_without_ext, _ = os.path.splitext(common_filename)

        # select all files with that pattern in the current directory
        command = f"scout -m {name_without_ext}"
        self.fm.execute_console(command)

        # Change mode to normal in case visual selection mode was on
        # self.fm.change_mode("normal")

        self.fm.notify(f"Done selecting pattern {name_without_ext}")

    def tab(self):
        return self._tab_directory_content()

###############################################################################

