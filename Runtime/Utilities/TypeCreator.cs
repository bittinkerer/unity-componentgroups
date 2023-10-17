using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Packages.Estenis.ComponentGroups_
{
	public class TypeCreator
	{
		public static T CreateTypeInstance<T>(string moduleName, string typeName)
			where T : MonoBehaviour
		{
			AssemblyName assemblyName = new($"{moduleName}_asm");
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.Run);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
			var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public, typeof(T));
			Type type = typeBuilder.CreateType();
			
			var instance = Activator.CreateInstance(type);

			return (T) instance;
		}
	}
}