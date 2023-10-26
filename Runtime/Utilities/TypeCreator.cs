using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;

namespace Packages.Estenis.ComponentGroups_
{
	public class TypeCreator
	{
		public static Type CreateType<T>(string moduleName, string typeName)
			where T : MonoBehaviour
		{
            AssemblyName assemblyName = new($"{moduleName}_asm");
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.Run);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
			var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public, typeof(T));
			Type type = typeBuilder.CreateType();
			return type;
			
		}
	}
}