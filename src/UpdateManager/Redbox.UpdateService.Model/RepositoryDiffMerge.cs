using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Redbox.Core;

namespace Redbox.UpdateService.Model;

public static class RepositoryDiffMerge
{
	private static readonly Dictionary<string, string> CompressionLibrary = new Dictionary<string, string> { { "%", "0000000000000000000000000000000000000000" } };

	public static bool TryDiff(List<ClientRepositoryDataDTO> input, List<ClientRepositoryDataDTO> output, bool verify, out RepositoryDiffWrapper rdw)
	{
		try
		{
			rdw = new RepositoryDiffWrapper();
			List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
			List<ClientRepositoryDataDTO> list2 = new List<ClientRepositoryDataDTO>();
			foreach (ClientRepositoryDataDTO item in output)
			{
				ClientRepositoryDataDTO i = item;
				ClientRepositoryDataDTO clientRepositoryDataDTO = input.FirstOrDefault((ClientRepositoryDataDTO fromItem) => i.R == fromItem.R);
				if (clientRepositoryDataDTO != null)
				{
					bool flag = false;
					Dictionary<string, string> dictionary = new Dictionary<string, string> { { "R", item.R } };
					if (item.H != clientRepositoryDataDTO.H)
					{
						dictionary.Add("H", item.H);
						flag = true;
					}
					if (item.I != clientRepositoryDataDTO.I)
					{
						dictionary.Add("I", item.I);
						flag = true;
					}
					if (item.A != clientRepositoryDataDTO.A)
					{
						dictionary.Add("A", item.A);
						flag = true;
					}
					if (item.S != clientRepositoryDataDTO.S)
					{
						dictionary.Add("S", item.S);
						flag = true;
					}
					if (flag)
					{
						list.Add(dictionary);
					}
				}
				else
				{
					list2.Add(item);
				}
			}
			List<string> d = (from inputItem in input
				where !output.Any((ClientRepositoryDataDTO outputItem) => inputItem.R == outputItem.R)
				select inputItem.R).ToList();
			rdw = new RepositoryDiffWrapper
			{
				O = Hash(output),
				I = Hash(input),
				U = list,
				A = list2,
				D = d
			};
			return !verify || Verify(input, rdw);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("There was an unhandled exception in TryDiff", e);
			rdw = null;
			return false;
		}
	}

	public static bool TryMerge(List<ClientRepositoryDataDTO> input, RepositoryDiffWrapper rdw, out List<ClientRepositoryDataDTO> output)
	{
		try
		{
			if (Hash(input) != rdw.I)
			{
				LogHelper.Instance.Log("RDS200", "The hash check of the input failed in merging");
				output = new List<ClientRepositoryDataDTO>();
				return false;
			}
			if (rdw.U != null && rdw.U.Any())
			{
				foreach (Dictionary<string, string> item in rdw.U)
				{
					ClientRepositoryDataDTO u = item.ToJson().ToObject<ClientRepositoryDataDTO>();
					ClientRepositoryDataDTO clientRepositoryDataDTO = input.First((ClientRepositoryDataDTO i) => u.R == i.R);
					if (u.H != null)
					{
						clientRepositoryDataDTO.H = u.H;
					}
					if (u.I != null)
					{
						clientRepositoryDataDTO.I = u.I;
					}
					if (u.A != null)
					{
						clientRepositoryDataDTO.A = u.A;
					}
					if (u.S != null)
					{
						clientRepositoryDataDTO.S = u.S;
					}
				}
			}
			if (rdw.A != null && rdw.A.Any())
			{
				input.AddRange(rdw.A);
			}
			if (rdw.D != null && rdw.D.Any())
			{
				rdw.D.ForEach(delegate(string i)
				{
					input.RemoveAll((ClientRepositoryDataDTO j) => i == j.R);
				});
			}
			output = input;
			if (Hash(output) != rdw.O)
			{
				LogHelper.Instance.Log("RDS100", "The hash check of the output failed in merging");
				output = new List<ClientRepositoryDataDTO>();
				return false;
			}
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("There was an unhandled exception in TryMerge", e);
			output = new List<ClientRepositoryDataDTO>();
			return false;
		}
	}

	public static string Hash(List<ClientRepositoryDataDTO> hashMe)
	{
		if (hashMe == null)
		{
			return string.Empty;
		}
		hashMe.Sort();
		return Encoding.ASCII.GetBytes(hashMe.ToJson()).ToASCIISHA1Hash();
	}

	public static string Minimize(RepositoryDiffWrapper wrapper)
	{
		ListDictionary listDictionary = new ListDictionary
		{
			{ "I", wrapper.I },
			{ "O", wrapper.O }
		};
		if (wrapper.U != null && wrapper.U.Any())
		{
			listDictionary.Add("U", wrapper.U);
		}
		if (wrapper.A != null && wrapper.A.Any())
		{
			listDictionary.Add("A", wrapper.A);
		}
		if (wrapper.D != null && wrapper.D.Any())
		{
			listDictionary.Add("D", wrapper.D);
		}
		string seed = listDictionary.ToJson();
		return CompressionLibrary.Aggregate(seed, (string current, KeyValuePair<string, string> i) => current.Replace(i.Value, i.Key));
	}

	public static RepositoryDiffWrapper Expand(string str)
	{
		str = CompressionLibrary.Aggregate(str, (string current, KeyValuePair<string, string> i) => current.Replace(i.Key, i.Value));
		return str.ToObject<RepositoryDiffWrapper>();
	}

	internal static List<ClientRepositoryDataDTO> Copy(this List<ClientRepositoryDataDTO> list)
	{
		return list.Select((ClientRepositoryDataDTO each) => each.Copy()).ToList();
	}

	private static bool Verify(List<ClientRepositoryDataDTO> input, RepositoryDiffWrapper rdw)
	{
		try
		{
			RepositoryDiffWrapper wrapper = rdw.Copy();
			List<ClientRepositoryDataDTO> input2 = input.Copy();
			wrapper = Expand(Minimize(wrapper));
			bool flag = TryMerge(input2, wrapper, out var _);
			if (!flag)
			{
				LogHelper.Instance.Log("RDS400", "Verification failed at merging.");
			}
			return flag;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("RDS401", "Verification failed with an unknown exception.", ex);
			return false;
		}
	}
}
