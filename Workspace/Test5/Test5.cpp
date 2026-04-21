#include <string>
#include <memory>
#include <vcclr.h>

using namespace System;

class Args
{
public: 
	Args() = default;
	Args(int i) 
	{ 
		_i = i; 
		Console::WriteLine("Args"); 
	}

	int _i = 0;
};

public ref class ConsoleArgs
{
public:
	ConsoleArgs() 
	{
		String^ s = "ConsoleArgs";
		Console::WriteLine(s); 
		_args = new Args(1);
	}
	~ConsoleArgs() 
	{
		delete _args;
	}

private:
	Args* _args = nullptr;
};

int main(void)
{
	ConsoleArgs ca1;
	ConsoleArgs^ ca2 = gcnew ConsoleArgs();
	std::unique_ptr<Args> _args = std::make_unique<Args>(2);
	return 0;
}
