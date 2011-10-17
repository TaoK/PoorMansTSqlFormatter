
#ifdef __cplusplus
extern "C" {
#endif /* __cplusplus */

/*****************************************************************************/

#include <stdlib.h>
#include <string.h>
#include <ctype.h>


#ifdef WIN32

/* WIN32 specific */
# define strcasecmp stricmp

#else /* WIN32 */

/* !WIN32 specific */
# define stricmp strcasecmp

/* It seems these are only available on windows */
char *strupr(char *s);
char *strlwr(char *s);
char *strrev(char *s);

#endif /* !WIN32 */


/* Find files/dirs in a dir, matching a wildcard pattern
   Returns a NULL delimited array of strings, which must be freed
   with free() (the array itself too) */
#define FINDFILES_FILES	0
#define FINDFILES_DIRS	1
#define FINDFILES_BOTH	2
char** find_files( const char* dir, const char *wc, int dirs );

/* Efficient strdup that concatenates strings. */
char* strdup2( const char* s1, const char* s2 );
char* strdup3( const char* s1, const char* s2, const char* s3 );
/* TODO: create strdup(...) */

/* Find memory block inside memory block */
char* memmem( const char* m1, size_t len1, const char *m2, size_t len2 );

/* Find and replace a char in a memory block */
char* memchrset( char* m1, size_t len1, int find, int replace );

/* Convert string of length 'size' to lower- or upper-case */
char* memlwr( char *ptr, size_t size );
char* memupr( char *ptr, size_t size );

/* Analyze the case of the characters (0 means no alphabetic chars) */
#define ANALYZECASE_UPPER	1
#define ANALYZECASE_LOWER	2
#define ANALYZECASE_MIXED	3
int analyze_case( const char* in, int inl );

/*****************************************************************************/

#ifdef __cplusplus
}
#endif /* __cplusplus */
