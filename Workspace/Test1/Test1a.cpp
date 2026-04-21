#include <format>
#include <iostream>
#include <string>
#include <vector>
#include "Test1.hpp"

//#pragma comment(lib, "LIBCMT.lib")

int test1()
{
	std::string str = "Hello test1!\n";
	std::cout << str;
	Test1 t1;
	return 42;
}
