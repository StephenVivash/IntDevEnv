#include "stdio.h"
#include "stdlib.h"
#include "string.h"

//#pragma comment(lib, "LIBCMT.lib")

int test2a()
{
	int a = 10;
	int b = 20;
	int c = a + b;
	char s1[128] = "The sum of %d and %d is %d\n";
	char s2[128] = "";
	strcpy(s2, s1);
	printf(s2, a, b, c);
	return 69;
}
