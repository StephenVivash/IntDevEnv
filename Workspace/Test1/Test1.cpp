#include <format>
#include <iostream>
#include <string>
#include <vector>

#include "Test1.hpp"

//#pragma comment(lib, "LIBCMT.lib")

int test1();

int main(int argc, char** argv)
{
    std::vector<int> vec = { 10, 20, 30, 40 };
    vec.push_back(50);

    std::string str = "Hello World!\n";
    std::cout << str;

    for (const auto& v : vec)
        std::cout << "vec = " << v << "\n";

    str = std::format("vec[0] = {}\n", vec[0]);
    std::cout << str;
    return test1();
}
