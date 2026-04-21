#include "stdio.h"
#include "stdlib.h"
#include "string.h"

#include "Test4.h"

//#pragma comment(lib, "LIBCMT.lib")

int Test4()
{
	char s1[128] = "Exported C function Test4\n";
	char s2[128] = "";
	strcpy(s2, s1);
	printf(s2);
	return 0;
}
