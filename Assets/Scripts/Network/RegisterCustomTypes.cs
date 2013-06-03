using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using ExitGames.Client.Photon;
using UnityEngine;

public static class RegisterCustomTypes
{
	private static Boolean RegisteredTypes;

	private static readonly Byte[] UsedValues = new[] {(Byte)'W', (Byte)'V', (Byte)'T', (Byte)'Q', (Byte)'P'};

	public static void Register()
	{
		if (RegisteredTypes)
			return;

		var Types = Assembly.GetExecutingAssembly().GetTypes().Where(Type => !Type.IsAbstract);
		var SerializableTypes = Types.Where(Type => Attribute.IsDefined(Type, typeof (PhotonSerializableAttribute)));

		Byte ByteCode = 64;
		foreach (var SerializableType in SerializableTypes)
		{
			while (UsedValues.Contains(ByteCode))
				ByteCode += 1;

			PhotonPeer.RegisterType(SerializableType, ByteCode, SerializeObject, DeserializeObject);
			//Debug.Log(String.Format("Registered type {0} with {1}", SerializableType, ByteCode));

			ByteCode += 1;
		}

		RegisteredTypes = true;
	}


	private static byte[] SerializeObject(object customobject)
	{
		var BinaryFormatter = new BinaryFormatter();
		var MemoryStream = new MemoryStream();
		BinaryFormatter.Serialize(MemoryStream, customobject);

		return MemoryStream.ToArray();
	}

	private static object DeserializeObject(byte[] bytes)
	{
		var BinaryFormatter = new BinaryFormatter();
		var MemoryStream = new MemoryStream(bytes);

		return BinaryFormatter.Deserialize(MemoryStream);
	}
}

public class PhotonSerializableAttribute : Attribute
{
}