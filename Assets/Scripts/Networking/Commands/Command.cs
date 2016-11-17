using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Networking.Commands
{
	public interface ICommand
	{
		void Execute(Model.ModelManager manager);
	}
}
