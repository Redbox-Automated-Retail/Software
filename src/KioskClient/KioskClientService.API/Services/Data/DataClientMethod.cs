using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;

namespace KioskClientService.API.Services.Data;

public delegate Task<APIResponse<DataModels.KioskDataResponse>> DataClientMethod(DataModels.KioskBaseRequest request);
