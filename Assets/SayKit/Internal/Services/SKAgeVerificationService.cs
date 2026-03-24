#if SAYKIT_CHINA_VERSION
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using SayKitInternal;
using UnityEngine;
using UnityEngine.UI;

[SuppressMessage("ReSharper", "ConvertToUsingDeclaration")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "NotAccessedField.Local")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
public class SKAgeVerificationService : MonoBehaviour
{
    private static SKAgeVerificationService _instance;

    private enum VerificationState
    {
        Default,
        Launch,
        ServerDateCheck,
        Passed,
        Failed
    }

    public InputField inputFieldForName;
    public InputField inputFieldForCode;
    public Button btnSubmit;
    public Button btnOk;
    public GameObject defaultPopup;
    public GameObject alertPopup;
    public Text alertText;

    private string sName;
    private string sCode;

    private const string appcode = "fe701c9aacc64322a24edef7ccb6c528";
    private const string SAYKIT_UNITY_AGE_VERIFICATION = "SAYKIT_UNITY_AGE_VERIFICATION";
    private const string SAYKIT_UNITY_AGE_VERIFICATION_ID = "SAYKIT_UNITY_AGE_VERIFICATION_ID";
    private const string TAG = "[SKAgeVerificationService]";

    private VerificationState state;
    private DateTime serverTime;
    private List<DateTime> _fesDateTimes;
    private Action _ageVerificationFinished;

    #region Models

    [Serializable]
    public class IpApiData
    {
        public string country_code;
        public string city;
    }

    [Serializable]
    public class AliUserInfo
    {
        public string code;
        public string msg;
        public string data;

        [NonSerialized] public Status deserializedData;

        public void DeserializeData()
        {
            if (!string.IsNullOrEmpty(data))
            {
                deserializedData = JsonUtility.FromJson<Status>(data);
            }
        }
    }

    [Serializable]
    public class Status
    {
        public Result data;
        public string seqNum;
        public string message;
        public int status;
    }

    [Serializable]
    public class Result
    {
        public int result;
        public string resultMsg;
    }

    #endregion

    #region AgeChecker

    private string GetBirthDay(string identityCard)
    {
        if (string.IsNullOrEmpty(identityCard))
        {
            return null;
        }

        if (identityCard.Length != 15 && identityCard.Length != 18)
        {
            return null;
        }

        string birthDay = null;
        if (identityCard.Length == 18)
        {
            birthDay = identityCard.Substring(6, 4) + "-" + identityCard.Substring(10, 2) + "-" +
                       identityCard.Substring(12, 2);
        }

        if (identityCard.Length == 15)
        {
            birthDay = "19" + identityCard.Substring(6, 2) + "-" + identityCard.Substring(8, 2) + "-" +
                       identityCard.Substring(10, 2);
        }

        return birthDay;
    }

    private int CalculateAge(string birthDay)
    {
        if (string.IsNullOrEmpty(birthDay)) return 0;
        var birthDate = DateTime.Parse(birthDay);
        var nowDateTime = DateTime.Now;
        var age = nowDateTime.Year - birthDate.Year;

        if (nowDateTime.Month < birthDate.Month ||
            (nowDateTime.Month == birthDate.Month && nowDateTime.Day < birthDate.Day))
        {
            age--;
        }

        return age;
    }

    private bool IsAdult(string identityCard)
    {
        if (CalculateAge(GetBirthDay(identityCard)) >= 18)
        {
            return true;
        }

        return false;
    }

    #endregion

    #region IDCardChecker

    private static bool CheckIDCard(string idNumber)
    {
        if (idNumber.Length == 18)
        {
            var check = CheckIDCard18(idNumber);
            return check;
        }

        if (idNumber.Length == 15)
        {
            var check = CheckIDCard15(idNumber);
            return check;
        }

        return false;
    }

    private static bool CheckIDCard18(string idNumber)
    {
        if (long.TryParse(idNumber.Remove(17), out var n) == false
            || n < Math.Pow(10, 16) || long.TryParse(idNumber.Replace('x', '0').Replace('X', '0'), out n) == false)
        {
            return false;
        }

        const string address =
            "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
        if (address.IndexOf(idNumber.Remove(2), StringComparison.Ordinal) == -1)
        {
            return false;
        }

        var birth = idNumber.Substring(6, 8).Insert(6, "-").Insert(4, "-");

        if (DateTime.TryParse(birth, out _) == false)
        {
            return false;
        }

        var arrVerifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
        var Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
        var Ai = idNumber.Remove(17).ToCharArray();

        var sum = 0;
        for (var i = 0; i < 17; i++)
        {
            sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
        }

        var y = -1;
        Math.DivRem(sum, 11, out y);

        if (arrVerifyCode[y] != idNumber.Substring(17, 1).ToLower())
        {
            return false;
        }

        return true;
    }

    private static bool CheckIDCard15(string idNumber)
    {
        if (long.TryParse(idNumber, out var n) == false || n < Math.Pow(10, 14))
        {
            return false;
        }

        const string address =
            "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
        if (address.IndexOf(idNumber.Remove(2), StringComparison.Ordinal) == -1)
        {
            return false;
        }

        var birth = idNumber.Substring(6, 6).Insert(4, "-").Insert(2, "-");
        if (DateTime.TryParse(birth, out _) == false)
        {
            return false;
        }

        return true;
    }

    #endregion

    public static SKAgeVerificationService GetInstance()
    {
        if (!_instance)
        {
            _instance =
                SKUtils.FindComponentOnRootObjects<SKAgeVerificationService>() ??
                Instantiate(SayKitAssets.Instance.SkAgeVerificationServicePopup);

            var canvas = _instance.GetComponent<Canvas>();
            SKUtils.SetupCanvas(canvas);

            DontDestroyOnLoad(_instance.gameObject);
            _instance.name = "[SKAgeVerification]";
        }

        return _instance;
    }

    public void StartVerification(Action action)
    {
        _ageVerificationFinished = action;

        if (SKManager.Instance.RemoteConfig.runtime.sk_age_verification_enabled == 0)
        {
            SKBridgeManager.Instance.TrackEvent(name: "sk_age_verification", extra1: "Age verification disabled by config.");
            _ageVerificationFinished?.Invoke();
            return;
        }

        SKBridgeManager.Instance.TrackEvent(name: "sk_age_verification_start");

        Init();
    }

    private void Init()
    {
        if (PlayerPrefs.GetInt(SAYKIT_UNITY_AGE_VERIFICATION, 0) == 1)
        {
            sCode = PlayerPrefs.GetString(SAYKIT_UNITY_AGE_VERIFICATION_ID);

            ServerDateCheck(
                Launch,
                () => { state = VerificationState.ServerDateCheck; }
            );
        }
        else
        {
            ShowPopUp();
        }

        btnSubmit.onClick.AddListener(CheckUserInfo);

        btnOk.onClick.AddListener(() =>
        {
            switch (state)
            {
                case VerificationState.ServerDateCheck:
                    state = VerificationState.Default;

                    ServerDateCheck(
                        Launch,
                        () => { state = VerificationState.ServerDateCheck; }
                    );
                    alertPopup.SetActive(false);
                    break;
                case VerificationState.Launch:
                    state = VerificationState.Default;

                    Launch();
                    alertPopup.SetActive(false);
                    break;
                case VerificationState.Failed:
                    state = VerificationState.Default;

                    SKBridgeManager.Instance.TrackEvent(name: "sk_age_verification_failed", extra1: "Not adult",
                        extra2: $"Name: {sName}, ID: {sCode}");

#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
                case VerificationState.Passed:
                    _ageVerificationFinished?.Invoke();
                    SKBridgeManager.Instance.TrackEvent(name: "sk_age_verification_passed", extra1: $"Name: {sName}, ID: {sCode}");
                    HidePopUp();
                    break;
                default:
                    state = VerificationState.Default;
                    alertPopup.SetActive(false);
                    break;
            }

            btnSubmit.interactable = true;
        });
    }

    private void ShowAlertPopUp(string message)
    {
        alertPopup.SetActive(true);

        if (!alertText.gameObject.activeSelf)
        {
            alertText.gameObject.SetActive(true);
        }

        alertText.text = message;
    }

    private void NotAdultAlert()
    {
        alertPopup.SetActive(true);

        if (alertText)
        {
            alertText.text = "根据国家防沉迷通知的相关要求 由于您是未成年人. 仅能在周五、周六、周日及法定节假日 20时至21时进入游戏";
        }
    }

    private void ShowPopUp()
    {
        defaultPopup.SetActive(true);
        alertPopup.SetActive(false);
    }

    private void HidePopUp()
    {
        defaultPopup.SetActive(false);
        alertPopup.SetActive(false);
    }

    private void CheckUserInfo()
    {
        btnSubmit.interactable = false;

        sName = inputFieldForName.text;
        sCode = inputFieldForCode.text;

        if (string.IsNullOrEmpty(sName))
        {
            ShowAlertPopUp("用户名不能为空");
            return;
        }

        if (string.IsNullOrEmpty(sCode))
        {
            ShowAlertPopUp("标识号不能为空");
            return;
        }

        if (!CheckIDCard(sCode))
        {
            ShowAlertPopUp("身份证号码格式不正确");
            return;
        }

        PlayerPrefs.SetString(SAYKIT_UNITY_AGE_VERIFICATION_ID, sCode);

        Verification();
    }

    private void Verification()
    {
        HttpWebResponse httpResponse = null;

        try
        {
            var body = "idcard=" + sCode + "&name=" + sName;
            const string url = "http://smrzxb.qianshutong.com/web/interface/smrzxb";

            var requestResponse = string.Empty;
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Method = "POST";
            httpRequest.Headers.Add("Authorization", "APPCODE " + appcode);
            httpRequest.Headers.Add("X-Ca-Nonce", Guid.NewGuid().ToString());
            httpRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            if (body.Length > 0)
            {
                var data = Encoding.UTF8.GetBytes(body);
                using (var stream = httpRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            var responseStream = httpResponse.GetResponseStream();
            if (responseStream != null)
            {
                var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                requestResponse = reader.ReadToEnd();
            }

            OnServerResponse(httpResponse.StatusCode, requestResponse);
        }
        catch (WebException e)
        {
            httpResponse = (HttpWebResponse)e.Response;
            ShowAlertPopUp("请求错误：" + e.Message);
            SayKitDebug.LogError($"{TAG}[Verification] WebException: {e.Message}, {e.StackTrace}");
        }
        catch (Exception e)
        {
            ShowAlertPopUp("请求错误：" + e.Message);
            ErrorHandler($"{TAG}[Verification] Exception: {e.Message}, {e.StackTrace}");
        }
    }

    private void OnServerResponse(HttpStatusCode requestStatusCode, string requestResponse)
    {
        try
        {
            if (requestStatusCode.ToString() != HttpStatusCode.OK.ToString())
            {
                ShowAlertPopUp("请求错误。代码：" + requestStatusCode);
            }
            else if (string.IsNullOrEmpty(requestResponse))
            {
                ShowAlertPopUp("数据错误");
            }
            else
            {
                var info = JsonUtility.FromJson<AliUserInfo>(requestResponse);
                info.DeserializeData();

                if (info.code == "200")
                {
                    switch (info.deserializedData.data.result)
                    {
                        case 1:
                            PlayerPrefs.SetInt(SAYKIT_UNITY_AGE_VERIFICATION, 1);
                            ServerDateCheck(
                                Launch,
                                () => { state = VerificationState.ServerDateCheck; }
                            );
                            break;
                        case 2:
                            PlayerPrefs.SetInt(SAYKIT_UNITY_AGE_VERIFICATION, 2);
                            state = VerificationState.Default;
                            ShowAlertPopUp(info.deserializedData.data.resultMsg);
                            break;
                        case 3:
                            PlayerPrefs.SetInt(SAYKIT_UNITY_AGE_VERIFICATION, 3);
                            state = VerificationState.Default;
                            ShowAlertPopUp(info.deserializedData.data.resultMsg);
                            break;
                        default:
                            state = VerificationState.Default;
                            ShowAlertPopUp(info.msg);
                            break;
                    }
                }
                else
                {
                    state = VerificationState.Default;
                    ShowAlertPopUp(info.msg + " Code:" + info.code);
                }
            }
        }
        catch (Exception e)
        {
            ErrorHandler($"{TAG}[OnServerResponse] Error: {e.Message}, {e.StackTrace}");
            state = 0;
            ShowAlertPopUp("未知错误");
        }
    }

    private void Launch()
    {
        if (!IsAdult(sCode))
        {
            if (!MinorsCheck())
            {
                InvokeRepeating(nameof(MinorsCheck), 1, 3);
            }
            else
            {
                NotAdultAlert();
                state = VerificationState.Passed;
            }
        }
        else
        {
            _ageVerificationFinished?.Invoke();
            SKBridgeManager.Instance.TrackEvent(name: "sk_age_verification_passed",
                extra1: $"Name: {sName}, ID: {sCode}");
            HidePopUp();
        }
    }

    private bool MinorsCheck()
    {
        if (!IsAdult(sCode))
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday ||
                DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Friday)
            {
                if (DateTime.Now.Hour != 20)
                {
                    CancelInvoke(nameof(MinorsCheck));
                    NotAdultAlert();
                    state = VerificationState.Failed;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                CancelInvoke(nameof(MinorsCheck));
                NotAdultAlert();
                state = VerificationState.Failed;
            }
        }
        else
        {
            CancelInvoke(nameof(MinorsCheck));
        }

        return false;
    }

    private void ServerDateCheck(Action onSuccess, Action onFail)
    {
        Dictionary<string, string> headerCollection = null;
        var serverTimeOK = false;

        try
        {
            var sayKitWebRequest = new SayKitWebRequest("https://www.baidu.com");
            sayKitWebRequest.SendAndWait(5);
            headerCollection = sayKitWebRequest.ResponseHeaders;

            foreach (var datetime in from key in sayKitWebRequest.ResponseHeaders.Keys
                     where key == "Date"
                     select headerCollection[key])
            {
                serverTime = Convert.ToDateTime(datetime);
                serverTimeOK = true;
                break;
            }

            if (serverTimeOK)
            {
                onSuccess?.Invoke();
            }
        }
        catch (Exception e)
        {
            serverTimeOK = false;
            onFail?.Invoke();

            alertPopup.SetActive(true);
            ShowAlertPopUp("服务器日期检查失败。建立网络连接失败。请确保您的设备已连接到互联网，然后重试。");
            ErrorHandler($"{TAG}[ServerDateCheck] Exception message: {e.Message}, stacktrace: {e.StackTrace}");
        }
        finally
        {
            headerCollection?.Clear();
        }
    }

    private void ErrorHandler(string message)
    {
        SayKitDebug.LogError(message);
        SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: message);
    }
}
#endif