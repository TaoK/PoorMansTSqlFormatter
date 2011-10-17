// 1998-2002 Lionello Lunesu.
//  
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Library General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.

//  FART <wc>					Show files that comply with <wc> + final count (find)

//  FART - <s>					Echo lines from stdin containing <s> + final count
//  FART -v - <s>				Echo lines from stdin NOT containing <s> + final count
//  FART <wc> <s>				Find files <wc>, echo lines containing <s> [+ count] (grep)
//  FART -v <wc> <s>			Find files <wc>, echo lines NOT containing <s> [+ count]

//  FART - <s> <r>				Echo lines from stdin containing <s>, but show <r> + final count
//  FART -v - <s> <r>			Echo lines from stdin containing <s>, but show <r> + final count
//  FART <wc> <s> <r>			Find files <wc> with <s>, replace with <r>, show filenames + count
//  FART -v <wc> <s> <r>		?
//  FART -p <wc> <s> <r>		Find files <wc> with <s>, show filenames, print lines with <r> + count
//  FART <wc> <s> "r"			Find files <wc>, remove lines with <s>, show filenames + count
//  FART -v <wc> <s> "r"		Find files <wc>, remove lines NOT with <s>, show filenames + count

// TODO:
// * don't touch files if nothing changed							done
// * prevent processing a file twice when fart'ing filenames		done
// * remove all references to _MAX_PATH								done
// * CVS-compatible file renaming
// * don't use temp file, unless needed
// * invert-mode for FART
// * rename folders
// * allow use of wildmat() on WIN32 too (but case-insensitive)
// * UNICODE version

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>

#include "fart_shared.h"

///////////////////////////////////////////////////////////////////////////////

#define VERSION				"v1.99b"

#define _WILDCARD_SEPARATOR	','
#define WILDCARD_ALL		"*"

#ifdef _WIN32

#include <io.h>							// for _setmode
#include <fcntl.h>						// for _O_BINARY
#include <process.h>					// for _spawn
//#include <conio.h>

# define _DRIVE_SEPARATOR	':'
# define _DIR_SEPARATOR		'\\'
# define DIR_CURRENT		""
# define DIR_SEPARATOR		"\\"
# define DIR_PARENT			"..\\"

#else // _WIN32

#include <unistd.h>						// for fork,execlp
#include <sys/wait.h>					// for wait

# define _DIR_SEPARATOR		'/'
# define DIR_CURRENT		"./"
# define DIR_SEPARATOR		"/"
# define DIR_PARENT			"../"

#endif // !_WIN32


// Output strings (eventually customizable)
static char __temp_file[16] = "_fart.~";
static char __backup_suffix[16] = ".bak";			// fart.cpp.bak
static char __linenumber[16] = "[%4i]";				// [   2]
static char __filename[16] = "%s\n";				// fart.cpp
static char __filename_count[16] = "%s [%i]\n";		// fart.cpp [2]
static char __filename_text[16] = "%s :\n";			// fart.cpp :
static char __filename_rename[16] = "%s => %s\n";	// fart.CPP => fart.cpp

// Option flags
bool	_Numbers = false;
bool	_Backup = false;
bool	_Preview = false;
bool	_Quiet = false;
bool	_Help = false;
bool	_IgnoreCase = false;
bool	_SubDir = false;
bool	_AdaptCase = false;
bool	_WholeWord = false;
bool	_CVS = false;
bool	_SVN = false;
bool	_Verbose = false;
bool	_Invert = false;
bool	_Count = false; 
bool	_Names = false; 
bool	_Binary = false;
bool	_CStyle = false;
bool	_Remove = false;

struct argument_t
{
	bool*	state;
	char	option;
	char*	option_long;
	char*	description;
} arguments[] = {
	// general options
	{ &_Help, 'h', "help", "Show this help message (ignores other options)" },
	{ &_Quiet, 'q', "quiet", "Suppress output to stdio / stderr" },
	{ &_Verbose, 'V', "verbose", "Show more information" },
//	{ &_Options, ' ', "", "No more options after this" },			// --
	// find options
	{ &_SubDir, 'r', "recursive", "Process sub-folders recursively" },
#ifdef _WIN32
	{ &_SubDir, 's', "subdir", NULL },
#endif
	{ &_Count, 'c', "count", "Only show filenames, match counts and totals" },
	// grep options
//	{ &_Regex, 'g', "regex", "Interpret find_string as a basic regular expression" },
	{ &_IgnoreCase, 'i', "ignore-case", "Case insensitive text comparison" },
	{ &_Invert, 'v', "invert", "Print lines NOT containing the find string" },
	{ &_Numbers, 'n', "line-number", "Print line number before each line (1-based)" },
	{ &_WholeWord, 'w', "word", "Match whole word (uses C syntax, like grep)" },
	{ &_Names, 'f', "filename", "Find (and replace) filename instead of contents" },
	{ &_Binary, 'B', "binary", "Also search (and replace) in binary files (CAUTION)" },
	{ &_CStyle, 'C', "c-style", "Allow C-style extended characters (\\xFF\\0\\t\\n\\r\\\\ etc.)" },
	// fart specific options
	{ &_CVS, ' ', "cvs", "Skip cvs dirs; execute \"cvs edit\" before changing files" },
	{ &_SVN, ' ', "svn", "Skip svn dirs" },
	{ &_Remove, ' ', "remove", "Remove all occurences of the find_string" },
//	{ &_VSS, ' ', 'vss", "Do SourceSafe check-out before changing r/o files" },
	{ &_AdaptCase, 'a', "adapt", "Adapt the case of replace_string to found string" },
	{ &_Backup, 'b', "backup", "Make a backup of each changed file" },
	{ &_Preview, 'p', "preview", "Do not change the files but print the changes" },
	// end
	{ NULL, 0, NULL, NULL } };

int		TotalFileCount;					// Number of files
int		TotalFindCount;					// Number of total occurences

#define MAXSTRING 8192

bool	HasWildCard = false;
char	WildCard[MAXSTRING];

bool	_DoubleCheck = false;
int		FindLength = 0;
char	FindString[MAXSTRING];
int		ReplaceLength = 0;
char	ReplaceString[MAXSTRING];

char	ReplaceStringLwr[MAXSTRING], ReplaceStringUpr[MAXSTRING];

char	fart_buf[MAXSTRING];

// Macro's for output to stderr, first flush stdout and later stderr
#define ERRPRINTF( s ) fflush(stdout),fprintf( stderr, s ),fflush(stderr)
#define ERRPRINTF1( s, a ) fflush(stdout),fprintf( stderr, s, a ),fflush(stderr)
#define ERRPRINTF2( s, a, b ) fflush(stdout),fprintf( stderr, s, a, b ),fflush(stderr)

///////////////////////////////////////////////////////////////////////////////
// Callback function for 'for_all_files' and 'for_all_files_recursive'

typedef int (*file_func_t)( const char* dir, const char* file );

///////////////////////////////////////////////////////////////////////////////

inline bool _iswordchar( char c )
{
//	return isalnum(c) || c=='_';
	return c=='_' || (c>='0'&&c<='9') || (c>='a'&&c<='z') || (c>='A'&&c<='Z');
}

///////////////////////////////////////////////////////////////////////////////
// Expands c-style character constants in the input string; returns new size
// (strlen won't work, since the string may contain premature \0's)

int cstyle( char *buffer )
{
	int len =0;
	char *cur = buffer;
	while (*cur)
	{
		if (*cur == '\\')
		{
			cur++;
			switch (*cur)
			{
			case 0: *buffer = '\0'; return len;
			case 'n': *buffer++ = '\n'; break;
			case 't': *buffer++ = '\t'; break;
			case 'v': *buffer++ = '\v'; break;
			case 'b': *buffer++ = '\b'; break;
			case 'r': *buffer++ = '\r'; break;
			case 'f': *buffer++ = '\f'; break;
			case 'a': *buffer++ = '\a'; break;
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			{
				int x, n=0;
				sscanf(cur,"%3o%n",&x,&n);
				cur += n-1;
				*buffer++ = (char)x;
				break;
			}
			case 'x':								// hexadecimal
			{
				int x, n=0;
				sscanf(cur+1,"%2x%n",&x,&n);
				if (n>0)
				{
					cur += n;
					*buffer++ = (char)x;
					break;
				}
				// seep through
			}
			default:  
				ERRPRINTF1( "Warning: unrecognized character escape sequence: \\%c\n", *cur );
			case '\\':
			case '\?':
			case '\'':
			case '\"':
				*buffer++ = *cur;
			}
			cur++;
		}
		else
			*buffer++ = *cur++;
		len++;
	}
	*buffer = '\0';
	return len;
}

///////////////////////////////////////////////////////////////////////////////
// Prepares a line for text comparison

const char* get_compare_buf( const char *in )
{
static char compare_buf[MAXSTRING];

	if (_IgnoreCase)
	{
		// Copy the text into an extra buffer (used just for comparison)
		strcpy( compare_buf, in );
		// Compare lowercase (assume FindString already is lowercase)
		strlwr( compare_buf );
		return compare_buf;
	}
	return in;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

void usage()
{
	// Print banner
	printf( "\nFind And Replace Text  %-30s by Lionello Lunesu\n\n",VERSION);

	printf("Usage: FART [options] [--] <wildcard>[%c...] [find_string] [replace_string]\n", _WILDCARD_SEPARATOR );
	printf("\nOptions:\n");
	for (int t=0;arguments[t].state;t++)
	{
		// don't print 'hidden' options
		if (!arguments[t].description)
			continue;
		if (arguments[t].option>' ')
			printf(" -%c,", arguments[t].option );
		else
			printf("    " );
		printf(" --%-14s%s\n", arguments[t].option_long, arguments[t].description );
	}
}

///////////////////////////////////////////////////////////////////////////////
// Returns 'true' if 'wc' is a file wildcard (containing * or ?)

bool is_wildcard( const char* wc )
{
	return strchr(wc,'*') || strchr(wc,'?');
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
// Method that prints a string with CR/LF temporarily cut off

void puts_nocrlf( char *_buf )
{
	int nl = strlen(_buf);

	while (nl>0 && (_buf[nl-1]=='\r' || _buf[nl-1]=='\n')) nl--;

	char o = _buf[nl];
	_buf[nl] = '\0';				// chop off the CR/LF
	puts(_buf);						// to stdout
	_buf[nl] = o;					// restore
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
// Returns the number of times the find string occurs in the input string

int findtext_line_count( const char *_line )
{
	const char *line = get_compare_buf(_line);

	int count = 0;
	const char *cur = line;
	const char *t;
	while ((t = strstr( cur, FindString )))
	{
		cur = t + FindLength;
		if (_WholeWord)
		{
			if (t>line && _iswordchar(t[-1]))
				continue;
			if (_iswordchar(t[FindLength]))
				continue;
		}
		count++;
	}
	return count;
}

///////////////////////////////////////////////////////////////////////////////
// Returns a pointer to the first occurence of the find string

const char* findtext_line( const char* _line )
{
	const char *line = get_compare_buf(_line);

	// Find the string in this line (FindString is lower case if _IgnoreCase)
	const char *cur = line;
	const char *t;
	while ((t = strstr( cur, FindString )))
	{
		if (_WholeWord)
		{
			cur = t + FindLength;
			if (t>line && _iswordchar(t[-1]))
				continue;
			if (_iswordchar(t[FindLength]))
				continue;
		}
		break;
	}
	return t;
}


///////////////////////////////////////////////////////////////////////////////
// Process input file while data available (filename is optional)

int _findtext( FILE* f, const char *filename )
{
	int	this_find_count=0;					// number of occurences in this file
	bool first = true;						// first occurence in this file?
	int ln=0;								// line number

	while (!feof(f))
	{
		ln++;
		if (!fgets( fart_buf, MAXSTRING, f ))
			break;

//		char *t = findtext_line( fart_buf );
		// no need to know the exact occurence, just count
		int t = findtext_line_count( fart_buf );

		if (_Invert)
		{
			if (t)
				continue;
			// count lines NOT containing the find_string
			t = 1;
		}
		else
		{
			if (!t)
				continue;
		}

		this_find_count += t;
			
		if (first)
		{
			// This is the first occurence in this file
			first = false;

			// If Q and C then we don't need the exact count, stop
			if (_Quiet && _Count)
				break;						// No need to continue

			if (!_Count && !_Quiet)
			{
				// We're dumping the lines after this
				if (filename)
					printf( __filename_text, filename );
			}
		}

		if (_Count)
			continue;

		if (_Numbers)
			printf( __linenumber, ln );

		puts_nocrlf( fart_buf );
	}
	return this_find_count;
}

///////////////////////////////////////////////////////////////////////////////

const char* pre_fart( const char* test )
{
	// TODO: make sure test[-1] is always valid and we can use this:
/*	if (_WholeWord)
	{
		if (t>compare_buf && _iswordchar(t[-1]))
			continue;
		if (_iswordchar(t[FindLength]))
			continue;
	}*/

	const char* replacement = ReplaceString;
	// find the correct (case-adapted) replace_string
	if (_AdaptCase)
	{
		int i = analyze_case(test,FindLength);
		if (i==ANALYZECASE_UPPER)
			replacement = ReplaceStringUpr;
		else
		if (i==ANALYZECASE_LOWER)
			replacement = ReplaceStringLwr;
	}

	// double-check to see whether anything will really changed
	if (_DoubleCheck)
		if (memcmp(test,replacement,FindLength)==0)
			return NULL;

	return replacement;
}

///////////////////////////////////////////////////////////////////////////////
// Find and replace text in one line. Returns NULL if nothing has changed

int fart_line( const char *_line, char *farted )
{
	const char *compare_buf = get_compare_buf(_line);

	farted[0]='\0';

	int count = 0;
	char *output = farted;			// output pointer
	size_t offset, cur = 0;

	// Find the string in this line (FindString is lower case if _IgnoreCase)
	for (const char *t;(t = strstr( compare_buf+cur, FindString ));cur=offset+FindLength)
	{
		offset = t - compare_buf;
		if (_WholeWord)
		{
			if (t>compare_buf && _iswordchar(t[-1]))
				continue;
			if (_iswordchar(t[FindLength]))
				continue;
		}

		const char* replacement = pre_fart(_line+offset);
		if (!replacement)
			continue;

		// string was found at t
		count++;
		// copy characters up-to t
		while (cur<offset)
			*output++ = _line[cur++];
		// append the replace string to the output
		for (int i=0;i<ReplaceLength;i++)
			*output++ = replacement[i];
		*output = '\0';
		// continue right after find string
	}
	if (count)
		// append the last part
		strcpy(output,_line+cur);
	return count;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
// Returns 'true' if the file seems binary, 'false' otherwise (text)

bool is_binary( FILE *f )
{
static char fout[256] = {0};

	if (!fout[0])
	{
		memset( fout, 1, 256 );					// not allowed
		memset( fout, 0, 128 );					// allowed <128
		memset( fout, 4, 32 );					// more expensive <32
		fout[0x9]=fout[0xA]=fout[0xD]=0;		// allowed: TAB CR LF
	}

	// check for binary file
	size_t tot = fread( fart_buf, 1, MAXSTRING, f );
	size_t noascii=0;
	for (size_t b=0;b<tot;b++)
	{
		unsigned char t = fart_buf[b];
		noascii += (size_t)fout[ t ];
	}

	// restore file pointer
	fseek(f,0,SEEK_SET);
//	freopen(in,"rb",f);

//	ERRPRINTF2("[%i/%i]",noascii,tot);
	return noascii*20>=tot;						// 5%
}

///////////////////////////////////////////////////////////////////////////////
// Find text in file
//  !DoKeepLine			Only show files NOT containing find_string
//  DoRemLine			Hide lines containing find_string

bool findtext_file( const char* in )
{
	// Open file for reading
	FILE *f = fopen(in,"rb");
	if (!f)
	{
		ERRPRINTF1( "Error: unable to open file: %s\n", in );
		return false;
	}

	// check binary/text
	if (!_Binary && is_binary(f))
	{
		if (_Verbose)
			ERRPRINTF1( "FART: skipping binary file: %s\n", in );
		fclose(f);
		return false;
	}

	int file_find_count = _findtext(f,in);
	if (file_find_count) TotalFileCount++;
	TotalFindCount += file_find_count;

	// Close file handle
	fclose(f);

	if (_Count && file_find_count)
	{
		// Show filenames [+ count]
		if (_Quiet)
			printf( __filename, in );
		else
			printf( __filename_count, in, file_find_count );
	}

	return true;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
// Find and replace text in file. Returns the number of replacements

int _fart( FILE *f1, FILE *f2, const char* in )
{
	int	this_find_count=0;					// Number of occurences in this file
	bool first = true;
	bool replace = false;
	int ln=0;

	// Process input file while data available
	while (!feof(f1))
	{
		ln++;
		if (!fgets( fart_buf, MAXSTRING, f1 ))
			break;

		const char* b = get_compare_buf(fart_buf);

		bool first_line=true;
		const char* bp = b;
		while (1)
		{
			const char *t = strstr(bp,FindString);

			// Check for word boundary
			if (t && _WholeWord)
			{
				// FIXME: does not search for another occurence
				if (t>bp && _iswordchar(t[-1]))
					t = NULL;
				else
				if (_iswordchar(t[FindLength]))
					t = NULL;
			}

			if (!t)
			{
				// find_string not found
				if (f2)
					fputs( fart_buf+(bp-b), f2 );
				break;
			}

			// Adapt the replace_string to the actually found string
			const char *replacement = pre_fart( fart_buf+(t-b) );
			if (!replacement)
			{
				if (f2)
					fputs( fart_buf+(bp-b), f2 );
				break;
			}

			this_find_count++;

			// First occurence in this file?
			if (first)
			{
				TotalFileCount++;
				first = false;
				// Print the filename
				if (!_Count && !_Quiet && in)
					printf( __filename, in );
			}

			// Remove lines containing the find_string
/*			if (DoRemLine)
			{
				replace = true;
				break;
			}
*/
			// Don't replace find_string if DoKeepLine is NOT set
/*			if (!DoKeepLine)
			{
				if (!t) 
				{
					// Skip this line (doesn't contain find_string)
					replace = true;
				}
				else
					fputs( fart_buf+(bp-b), f2 );
				break;
			}
*/
			// First occurence in this line?
			if (first_line)
			{
				if (_Numbers)
					printf(__linenumber, ln );
//				if (!_Quiet)
//					puts_nocrlf(fart_buf);
				first_line = false;
			}

			// TODO: no temp file yet? create one and copy data up to here
//			if (!f2)

			// Write the text before the find_string
			fwrite( fart_buf+(bp-b), 1, t-bp, f2 );
			// Write the replace_string instead of the find_string
			fwrite( replacement, 1, ReplaceLength, f2 );

			// Put the buffer-pointer right after the find_string
			bp = t + FindLength;

			// Something got replaced
			replace = true;
		}
	}
	return replace?this_find_count:0;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
// Find and replace text in file (uses temporary file)

bool fart( const char* in )
{
	// Open original file for reading
	FILE *f1 = fopen(in,"rb");
	if (!f1)
	{
		ERRPRINTF1( "Error: unable to open file: %s\n", in );
		return false;
	}

	// check binary/text
	if (!_Binary && is_binary(f1))
	{
		if (_Verbose)
			ERRPRINTF1( "FART: skipping binary file: %s\n", in );
		fclose(f1);
		return false;
	}

	// Open temporary file for writing
	FILE *f2 = fopen(__temp_file,"wb");
	if (!f2)
	{
		ERRPRINTF( "Error: unable to create temporary file\n" );
		fclose(f1);
		return false;
	}

	int this_find_count = _fart(f1,f2,in);

	// Close file handles
	fclose(f2);
	fclose(f1);

	if (this_find_count && _Count && !_Quiet)
	{
		// Show filenames + count
		printf( __filename_count, in, this_find_count );
	}

	if (this_find_count && !_Preview)
	{
		TotalFindCount += this_find_count;
		if (_CVS)
		{
			if (_Verbose)
				ERRPRINTF1( "FART: cvs edit %s\n", in );
#ifdef _WIN32
			// win32: check for read-only first?
			_spawnlp( _P_WAIT, "cvs", "cvs", "edit", in, NULL );
#else
			if (fork()==0)
			{
				// child process; execute "cvs edit" (will not return)
				execlp( "cvs", "cvs", "edit", in, NULL );
			}
			wait(NULL);
#endif
			// TODO: check return code; stop on failure
		}

		if (_Backup)
		{
			// Append ".bak" to filename
			char *backup = strdup2(in,__backup_suffix);
			// Remove old backup. Rename original file to backup-filename
			if (remove( backup )!=0)
				ERRPRINTF1( "Error: could not remove: %s\n", backup );
			else
			if (rename( in, backup )!=0)
				ERRPRINTF1( "Error: could not backup: %s\n", in );
			free(backup);
		}
		else
		{
			// Remove original file
			if (remove( in )!=0)
				ERRPRINTF1( "Error: file is read only: %s\n", in );
		}
		// Rename temporary file to original filename
		//  (will fail if we could not rename/remove the original file)
		rename( __temp_file, in );
	}

	// Remove temporary file
	// (either nothing has changed, or we failed to rename/remove the original file)
	remove( __temp_file );

	return true;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
// Search for files matching a wildcard
// FIXME: when fart'ing filenames, we might process a file twice!

int for_all_files( const char *dir, const char* wc, file_func_t _ff )
{
	char ** spul = find_files(dir,wc,FINDFILES_FILES);
	if (!spul)
		return 0;

	if (_Verbose)
		ERRPRINTF2( "FART: processing %s,%s\n",dir,wc);

	int count = 0;
	for (int t=0;spul[t];t++)
	{
static char prog[] = "|/-||";
static int progress = 0;
		fprintf(stderr,"%c\r",prog[progress]); if (!prog[++progress]) progress=0;
		count += _ff( dir, spul[t] );
		free( spul[t] );
	}

	free(spul);
	return count;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
// Recurse through directory and search for files matching a wildcard

int for_all_files_recursive( const char *dir, const char* wc, file_func_t _ff )
{
	// First, process the current directory
	int count = for_all_files(dir,wc,_ff);

	// Now we recurse into folders
	char ** spul = find_files(dir,WILDCARD_ALL,FINDFILES_DIRS);
	if (!spul)
		return 0;

	for (int t=0;spul[t];free(spul[t]),t++)
	{
		// Skip "."
		if (!strcmp(spul[t],"."))
			continue;
		// Skip ".."
		if (!strcmp(spul[t],".."))
			continue;
		// Don't recurse into cvs directories
		if (_CVS && strcmp(spul[t],"CVS")==0)
		{
			if (_Verbose)
				ERRPRINTF2( "FART: skipping cvs dir %s%s\n",dir, spul[t] );
			continue;
		}
		// Don't recurse into svn directories
		if (_SVN && strcmp(spul[t],".svn")==0)
		{
			if (_Verbose)
				ERRPRINTF2( "FART: skipping svn dir %s%s\n",dir, spul[t] );
			continue;
		}

		char *_path = strdup3(dir,spul[t],DIR_SEPARATOR);
		count += for_all_files_recursive(_path,wc,_ff);
		free(_path);
	}
	free(spul);
	return count;
}

///////////////////////////////////////////////////////////////////////////////

int print_files( const char* dir, const char* file )
{
	char *_path = strdup2(dir,file);
/*	if (!_Preview)
	{
		FILE *f = fopen(_path,"rb");
		if (!f)
		{
			ERRPRINTF1("Error: could not access file: %s\n", _path);
			free(_path);
			return 0;
		}
		fclose(f);
	}*/
	puts(_path);
	free(_path);
	return 1;
}

///////////////////////////////////////////////////////////////////////////////

int for_all_files_smart( const char* dir, const char* file, file_func_t _ff )
{
//	if (!is_wildcard(file) && !_SubDir)
//		return _ff(dir,file);

	if (_SubDir)
		return for_all_files_recursive( dir, file, _ff );
	else
		return for_all_files( dir, file, _ff );
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

int for_all_wildcards( char *wildcard, file_func_t _ff )
{
	int count = 0;
	while (1)
	{
		char *wc_sep = strchr(wildcard,_WILDCARD_SEPARATOR);
		if (wc_sep)
			*wc_sep = '\0';

		char *dir_sep = strrchr(wildcard,_DIR_SEPARATOR);
#ifdef _DRIVE_SEPARATOR
		if (!dir_sep)
			dir_sep = strchr(wildcard,_DRIVE_SEPARATOR);
#endif
		if (dir_sep)
		{
			dir_sep++;					// points to filename after slash
			if (*dir_sep)				// filename available?
			{
				char *path = strdup(wildcard);
				path[dir_sep - wildcard] = '\0';
				count += for_all_files_smart( path, dir_sep, _ff );
				free(path);
			}
			else
			{
				// No wildcard, assume ALL
				count += for_all_files_smart( wildcard, WILDCARD_ALL, _ff );
			}
		}
		else
		if (strcmp(wildcard,".")==0)
			count += for_all_files_smart( DIR_CURRENT, WILDCARD_ALL, _ff );
		else
		if (strcmp(wildcard,"..")==0)
			count += for_all_files_smart( DIR_PARENT, WILDCARD_ALL, _ff );
		else
			count += for_all_files_smart( DIR_CURRENT, wildcard, _ff );

		// No separator found? Finished.
		if (!wc_sep)
			break;

		*wc_sep = _WILDCARD_SEPARATOR;		// restore
		wildcard = wc_sep + 1;				// next piece
	}
	return count;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

void options_short( const char *options )
{
	while (options[0])
	{
//		char opt = _tolower(argv[t][1]);
		int tt;
		for (tt=0;arguments[tt].state;tt++)
			if (arguments[tt].option == options[0])
			{
				*arguments[tt].state = true;
				if (_Verbose)
					ERRPRINTF1( "FART: --%s\n", arguments[tt].option_long );
				break;
			}
		// Did we process the option?
		if (!arguments[tt].state)
		{
			if (options[0]!='?')				// don't show error for '?' 
				ERRPRINTF1( "Error: invalid option -%c\n", options[0] );
			_Help = true;
		}
		// Next option
		options++;
	}
}

///////////////////////////////////////////////////////////////////////////////

void options_long( const char *option )
{
	for (int tt=0;arguments[tt].state;tt++)
		if (strcmp(arguments[tt].option_long,option)==0)
		{
			*arguments[tt].state = true;
			if (_Verbose)
				ERRPRINTF1( "FART: --%s\n", arguments[tt].option_long );
			return;
		}
	ERRPRINTF1( "Error: invalid option --%s\n", option );
	_Help = true;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

void parse_options( int argc, char* argv[] )
{
	bool do_options = true;
	for (int t=1;t<argc;t++)
	{
#ifdef _WIN32
		// Parse DOS style options
		if (do_options && argv[t][0]=='/')
		{
			options_short(argv[t]+1);
			continue;
		}
#endif

		// Parse options; "-" is NOT an option but file/text
		if (do_options && argv[t][0]=='-' && argv[t][1])
		{
			if (argv[t][1]=='-')
			{
				// Long option; no other options appear after "--"
				if (argv[t][2])
					options_long( argv[t]+2 );
				else
					do_options = false;
			}
			else
				options_short( argv[t]+1 );
			continue;
		}

		// Check for wildcard first
		if (!HasWildCard)
		{
			if (_Verbose)
				ERRPRINTF1( "FART: wild_card=\"%s\"\n", argv[t] );
			strcpy( WildCard, argv[t] );
			HasWildCard = true;
			continue;
		}

		// Check for find_string next
		if (!FindLength)
		{
			if (_Verbose)
				ERRPRINTF1( "FART: find_string=\"%s\"\n", argv[t] );
			FindLength = strlen(argv[t]);
			memcpy( FindString, argv[t], FindLength+1 );
			continue;
		}

		// Check for replace_string next
		if (!ReplaceLength)
		{
			if (_Verbose)
				ERRPRINTF1( "FART: replace_string=\"%s\"\n", argv[t] );
			ReplaceLength = strlen(argv[t]);
			memcpy( ReplaceString, argv[t], ReplaceLength+1 );
			continue;
		}

		ERRPRINTF1( "Error: redundant argument \"%s\".\n", argv[t] );
		_Help = true;
	}
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

int findtext_file_path( const char* dir, const char* file )
{
	char *_path = strdup2(dir,file);

	if (_Names)
	{
		int count = findtext_line_count(file);
		if (_Invert) count = !count;
		if (count)
		{
			// file name contains pattern; print filename
			TotalFindCount+=count;
			TotalFileCount++;
			puts(_path);
		}
	}
	else
	{
		// check file contents
		findtext_file( _path );
	}

	free(_path);
	return 1;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

int fart_file_path( const char* dir, const char* file )
{
	char *_path = strdup2(dir,file);

	if (_Names)
	{
		int count = fart_line(file,fart_buf);
		if (count)
		{
			// fart_buf contains the new filename
			TotalFileCount++;

			char *newpath = strdup2(dir,fart_buf);
			// What about renaming files 'in' CVS?
			if (_Preview || rename( _path, newpath )==0)
			{
				// Filename was changed (only increment count if actually done)
				if (!_Preview)
					TotalFindCount += count;
				printf( __filename_rename, _path, fart_buf);
			}
			else
			{
				ERRPRINTF2("Error: could not rename %s to %s\n", _path, fart_buf );
				// read-only: CVS?
			}
			free(newpath);
		}
	}
	else
	{
		// find and replace in file contents
		fart( _path );
	}

	free(_path);
	return 1;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

int main( int argc, char* argv[] )
{
	parse_options( argc, argv );

	if (_Help || !HasWildCard)
	{
		// S/He obviously needs some help
		usage();
		return -1;
	}

	if (!FindLength)
	{
		// FIND-mode: search for files matching the wildcard
		int i = for_all_wildcards( WildCard, &print_files );
		if (!_Quiet)
			printf("Found %i file(s).\n",i);
		return i;
	}

#ifdef _WIN32
	if (_Binary)
	{
		// Switch stdin/out to binary mode
		_setmode( _fileno(stdin), _O_BINARY );
		_setmode( _fileno(stdout), _O_BINARY );
	}
#endif

	TotalFileCount = TotalFindCount = 0;

	// Warn for non-critical conflicting options
	if (_Count && _Numbers)
		ERRPRINTF( "Warning: conflicting options: --line-number, --count\n" );

	// Expand c-style character constants
	if (_CStyle && FindLength)
	{
		FindLength = cstyle(FindString);
		if (_Verbose)
			ERRPRINTF1( "FART: actual find_length=%i\n", FindLength );
	}
	if (_CStyle && ReplaceLength)
	{
		ReplaceLength = cstyle(ReplaceString);
		if (_Verbose)
			ERRPRINTF1( "FART: actual replace_length=%i\n", ReplaceLength );
	}

	// Case insensitive: we compare in lower case
	if (_IgnoreCase && FindLength)
		strlwr(FindString);									// FIXME: memlwr

	bool grepMode = (ReplaceLength==0);						// grep or fart?

	// OPTIMIZE: Check for redundant FART (where find_string==replace_string)
	if (ReplaceLength && FindLength==ReplaceLength)
	{
		if (!_IgnoreCase && memcmp(FindString,ReplaceString,FindLength)==0)
		{
			ERRPRINTF( "Warning: strings are identical.\n");
			grepMode = true;								// 'grep' mode
		}
	}

	if (_Remove)
	{
		if (ReplaceLength)
		{
			ERRPRINTF( "Error: option --remove conflicts with replace_string\n" );
			return -4;
		}
		// ReplaceString was not initialized by parse_options; do it here
		ReplaceString[0] = 0;
		grepMode = false;									// fart mode
	}

	if (grepMode)
	{
		// GREP-mode
		if (strcmp( WildCard, "-" )==0)
		{
			// Find text in stdin
			// If asked to check filenames, just return 0 (pointless)
			int count = _Names?0:_findtext( stdin, NULL );
			if (!_Quiet)
				printf( "Found %i occurence(s).\n", count );
			return count;
		}

		// Find text in files
		for_all_wildcards( WildCard, &findtext_file_path );
		if (!_Quiet)
			printf( "Found %i occurence(s) in %i file(s).\n", TotalFindCount, TotalFileCount);

		return TotalFindCount;
	}


	// FART-mode

	memcpy( ReplaceStringLwr, ReplaceString, ReplaceLength+1 );
	strlwr( ReplaceStringLwr );							// FIXME: memlwr

	if (_AdaptCase)
	{
		if (_IgnoreCase)
		{
//			memcpy( ReplaceStringLwr, ReplaceString, ReplaceLength+1 );
//			strlwr( ReplaceStringLwr );							// FIXME: memlwr
			memcpy( ReplaceStringUpr, ReplaceString, ReplaceLength+1 );
			strupr( ReplaceStringUpr );							// FIXME: memlwr
			// We now have 3 strings: Lower, Mixed and Upper
		}
		else
		{
			// OPTIMIZE: We only need to adapt the replace_string once
			int i = analyze_case(FindString,FindLength);
			if (i==ANALYZECASE_LOWER)
				strlwr(ReplaceString);							// FIXME: memlwr
			else
			if (i==ANALYZECASE_UPPER)
				strupr(ReplaceString);							// FIXME: memlwr
			if (i && _Verbose)
				ERRPRINTF1( "FART: actual replace_string=\"%s\"\n", ReplaceString );
			_AdaptCase = false;
		}
	}

	// OPTIMIZE: double-check to see whether anything really changed
	if (_IgnoreCase && FindLength==ReplaceLength)
		_DoubleCheck = memcmp(ReplaceStringLwr,FindString,FindLength)==0;

	if (strcmp( WildCard, "-" )==0)
	{
		// FART in stdin/stdout
		// If asked to check filenames, just return 0 (pointless)
		int count = _Names?0:_fart( stdin, stdout, NULL );
		if (!_Quiet)
			printf( "Replaced %i occurence(s).\n", count );
		return count;
	}

	if ((_CVS || _SVN) && _Names)
	{
		ERRPRINTF("Error: renaming version controlled files would destroy their history\n");
		return -3;
	}
	
	if (_Binary && !_Preview)
	{
		// If the size changes, binary files will very likely stop working
		if (/*FindLength!=ReplaceLength &&*/ !_Backup)
		{
			ERRPRINTF( "Error: too dangerous; must specify --backup" );
			return -2;
		}
		// Warn about FART'ing binary files
		ERRPRINTF( "Warning: fart may corrupt binary files\n" );
	}

	for_all_wildcards( WildCard, &fart_file_path );
	if (!_Quiet)
		printf( "Replaced %i occurence(s) in %i file(s).\n", TotalFindCount, TotalFileCount);

	return TotalFindCount;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
