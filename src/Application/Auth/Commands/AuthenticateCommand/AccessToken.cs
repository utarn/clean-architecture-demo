namespace MyAuthorizationDemo.Application.Auth.Commands.AuthenticateCommand;

public class AccessToken
{
    /// <summary>
    ///     โทเค็น JWT สำหรับใช้เข้าถึงส่วนงานอื่น
    /// </summary>
    /// <example>yJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJmYjc4NThhZS05MTZmLTQzOWQtYmU3ZS03ZDRkOTI1ZWY4NDEiLCJ1bmlxdWVfbmFtZSI6InV0YXJuMSIsInJvbGUiOiJBUElVc2VyIiwibmJmIjoxNjQ0MjM2NTQ5LCJleHAiOjE2NDQyNDAxNDksImlhdCI6MTY0NDIzNjU0OX0.kcFC08OPprbXpJJFc89LCDkJKDC_yBE1rVqsYeugmX8</example>
    public string Token { get; set; } = default!;
}
