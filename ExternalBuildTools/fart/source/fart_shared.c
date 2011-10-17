#include "fart_shared.h"

#ifdef _WIN32

#include <io.h>
#include <direct.h>
#define USE_WILDMAT

#else /* _WIN32 */

#include <dirent.h>
#define USE_WILDMAT

#ifndef DT_DIR
#include <sys/stat.h>
#endif

#endif /* !_WIN32 */


#ifdef USE_WILDMAT
/*extern "C"*/ 
int wildmat (const char *text, const char *p);
#endif

/*****************************************************************************/

int analyze_case( const char* in, int inl )
{
	int t, UC=0, LC=0;

	for (t=0;t<inl;t++)
	{
		char uc = toupper(in[t]);
		char lc = tolower(in[t]);

		if (uc==lc)						/* char is non-alphabetic	*/
			continue;

		if (uc==in[t])					/* char is uppercase	*/
			UC++;
		else							/* char is lowercase	*/
			LC++;
	}

	if (UC==0 && LC==0)					/* no alphabetic chars	*/
		return 0;

	/* If all the chars are either upper- or lowercase			*/
	/* we can get rid of the 'unknown' cases					*/

	if (UC==0)
		return ANALYZECASE_LOWER;		/* all lowercase		*/

	if (LC==0)
		return ANALYZECASE_UPPER;		/* all uppercase		*/

	return ANALYZECASE_MIXED;
}

/*****************************************************************************/

char* memlwr( char *ptr, size_t size )
{
	char *p;
	for (p=ptr;size--;p++)
		*p = tolower(*p);
	return ptr;
}

char* memupr( char *ptr, size_t size )
{
	char *p;
	for (p=ptr;size--;p++)
		*p = toupper(*p);
	return ptr;
}

/*****************************************************************************/

char* memmem( const char* m1, size_t len1, const char *m2, size_t len2 )
{
	size_t c,t;
	if (len1<len2)
		return NULL;
	/* Check for valid arguments (same behaviour as strstr) */
	if (!m2 || !len2)
		return (char*)m1;

	for (t=0;t<=len1-len2;t++)
	{
		for (c=0;c<len2;c++)
			if (m1[c+t]!=m2[c])
				break;
		if (c==len2)
			return (char*)&m1[t];
	}
	return NULL;
}

/*****************************************************************************/

char* strdup2( const char* s1, const char* s2 )
{
	int l1 = strlen(s1);
	int l2 = strlen(s2) + 1;
	char *str = (char*)malloc(l1+l2);
	memcpy( str, s1, l1 );
	memcpy( str+l1, s2, l2 );
	return str;
}

char* strdup3( const char* s1, const char* s2, const char* s3 )
{
	int l1 = strlen(s1);
	int l2 = strlen(s2);
	int l3 = strlen(s3) + 1;
	char *str = (char*)malloc(l1+l2+l3);
	memcpy( str, s1, l1 );
	memcpy( str+l1, s2, l2 );
	memcpy( str+l1+l2, s3, l3 );
	return str;
}

/*****************************************************************************/

#ifdef _WIN32

char** find_files( const char* dir, const char *wc, int dirs_or_files )
{
	char *_path;
	long fh;
	struct _finddata_t fd;
	char **spul;
	int numitems;
#ifdef USE_WILDMAT
	/* When using the wildmat routine, we compare in lowercase */
	char *wclwr, *namelwr;
	int matched;
#endif;

	/* Make full path wildcard */
#ifdef USE_WILDMAT
	_path = strdup2(dir,"*");
#else
	_path = strdup2(dir,wc);
#endif
	fh = _findfirst( _path, &fd );
	free(_path);

	if (fh==-1)
		return NULL;

#ifdef USE_WILDMAT
	wclwr = strlwr(strdup(wc));
#endif

	spul = NULL;
	numitems = 0;
	do
	{
		if (!(fd.attrib & _A_SUBDIR)==dirs_or_files)
			continue;
#ifdef USE_WILDMAT
		namelwr = strlwr(strdup(fd.name));
		matched = !strcmp(namelwr,wclwr) || wildmat(namelwr,wclwr);
		free(namelwr);
		if (!matched)
			continue;
#endif
		spul = (char**)realloc(spul, ++numitems*sizeof(char*));
		spul[numitems-1] = strdup(fd.name);
	}
	while (_findnext(fh,&fd)==0);

	_findclose(fh);

	/* append NULL */
	spul = (char**)realloc(spul, ++numitems*sizeof(char*));
	spul[numitems-1] = NULL;

#ifdef USE_WILDMAT
	free( wclwr );
#endif

	return spul;
}

#else /* _WIN32 */

char** find_files( const char* dir, const char *wc, int dirs_or_files )
{
	DIR *hd;
	struct dirent* dirent;
	char **spul;
	int numitems;
#ifndef DT_DIR
	/* dirent without d_type field; we must 'stat' to get the type */
	struct stat sbuf;
	char *_fullpath;
	int i;
#endif

	/* Make full path wildcard */
	hd = opendir(dir);
	if (!hd)
		return NULL;

	dirent = readdir(hd);
	if (!dirent)
	{
		closedir(hd);
		return NULL;
	}
	spul = NULL;
	numitems = 0;

	do 
	{
		/* Do files now; process folders later */
#ifdef DT_DIR
		if (!(dirent->d_type==DT_DIR)==dirs_or_files)
			continue;
#else
		if (dirs_or_files!=FINDFILES_BOTH)
		{
			_fullpath = strdup2(dir,dirent->d_name);
			i = stat(_fullpath, &sbuf);
			free(_fullpath);
			/* skip entry if stat failed or if it's not what we want */
			if (i==-1 || !S_ISDIR(sbuf.st_mode)==dirs_or_files)
				continue;
		}
#endif

#ifdef USE_WILDMAT
		if (strcmp(dirent->d_name,wc) && !wildmat(dirent->d_name,wc))
			continue;
#endif
		spul = (char**)realloc(spul, ++numitems*sizeof(char*));
		spul[numitems-1] = strdup(dirent->d_name);
	}
	while ((dirent = readdir(hd)));

	closedir(hd);

	/* append NULL */
	spul = (char**)realloc(spul, ++numitems*sizeof(char*));
	spul[numitems-1] = NULL;

	return spul;
}

/***************************************************************************/

char *strupr(char *s)
{
	int t;
	for (t=0;s[t];t++)
		s[t] = toupper(s[t]);
	return s;
}


char *strlwr(char *s)
{
	int t;
	for (t=0;s[t];t++)
		s[t] = tolower(s[t]);
	return s;
}

/*****************************************************************************/

#endif /* !_WIN32 */

/*****************************************************************************/

/*
struct string {
	string*		next;
	char		text[1];
};

string* alloc_string( string *here, const char *s )
{
	int i = strlen(s);
	string *ns = (string*)malloc(sizeof(string)+i);
	memcpy(ns->text,s,i+1);
	if (here)
	{
		ns->next = here->next;
		here->next = ns;
	}
	else 
		ns->next = NULL;
	return ns;
}

void free_string( string *s )
{
	if (s) free_string( s->next );
	free( (void*) s);
}

typedef long HDIR;
# define HDIR_FIRST(dir,ent,path)	dir=_findfirst(path,&ent),dir!=-1
# define HDIR_NEXT(dir,ent)			_findnext(dir,&ent)!=-1
# define HDIR_CLOSE(dir)			_findclose(dir)

typedef _finddata_t HDIRENT;
# define HDIRENT_NAME(ent)	(ent).name
# define HDIRENT_ISDIR(ent)	((ent).attrib & _A_SUBDIR)

#else // _WIN32

# define DIR_SEPARATOR		'/'
# define DIR_CURRENT		""
# define DIR_PARENT			"../"
# define USE_WILDMAT

typedef DIR* HDIR;
# define HDIR_FIRST(dir,ent,path)	dir=opendir(path)?DIR_NEXT(dir,ent):false
# define HDIR_NEXT(dir,ent)			ent=readdir(dir),ent!=NULL
# define HDIR_CLOSE(dir)			closedir(dir)

typedef struct dirent* HDIRENT;
# define HDIRENT_NAME(ent)	(ent)->d_name
# define HDIRENT_ISDIR(ent)	((ent)->d_type==DT_DIR)
*/

/*****************************************************************************/
