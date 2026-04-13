namespace NotificationService.Application.EmailTemplates;

public static class EmailTemplateBuilder
{
    public static string Build(string headerTitle, string contentHtml)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 20px;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 15px rgba(0,0,0,0.05);
        }}
        .header {{
            background: linear-gradient(to bottom, #fdf8f6, #faeae8);
            padding: 40px 20px 30px 20px;
            text-align: center;
            border-bottom: 3px solid #e47a8c;
            font-family: 'Times New Roman', Times, serif;
        }}
        .header .pre-title {{
            color: #3c281e;
            font-size: 32px;
            font-weight: 600;
            margin: 0;
            line-height: 1.2;
        }}
        .header .brand {{
            color: #e47a8c;
            font-size: 42px;
            font-style: italic;
            font-weight: bold;
            margin: 5px 0 15px 0;
            line-height: 1.2;
        }}
        .header .tagline {{
            color: #665b55;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            font-size: 13px;
            margin: 0;
            line-height: 1.5;
        }}
        .content-header {{
            background-color: #1a1a2e;
            color: #fff;
            padding: 15px 30px;
            text-align: center;
        }}
        .content-header h1 {{
            margin: 0;
            font-size: 20px;
            font-weight: 500;
            letter-spacing: 1px;
        }}
        .content {{
            padding: 40px 30px;
            line-height: 1.6;
        }}
        .content h2 {{
            color: #1a1a2e;
            margin-top: 0;
        }}
        .item-table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
            margin-bottom: 20px;
        }}
        .item-table th, .item-table td {{
            text-align: left;
            padding: 12px;
            border-bottom: 1px solid #eee;
        }}
        .item-table th {{
            background-color: #f9f9f9;
            color: #666;
            font-weight: 600;
        }}
        .grand-total {{
            font-size: 18px;
            font-weight: bold;
            text-align: right;
            padding-top: 20px;
            color: #1a1a2e;
        }}
        .footer {{
            background-color: #f9f9f9;
            padding: 20px;
            text-align: center;
            font-size: 12px;
            color: #888;
            border-top: 1px solid #eee;
        }}
        .btn {{
            display: inline-block;
            background-color: #e94560;
            color: #fff;
            text-decoration: none;
            padding: 12px 25px;
            border-radius: 4px;
            margin-top: 20px;
            font-weight: bold;
        }}
        .info-box {{
            background-color: #f8f9fa;
            border-left: 4px solid #e94560;
            padding: 15px;
            margin: 20px 0;
        }}
        .info-box p {{
            margin: 0;
            padding: 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <p class='pre-title'>Welcome to</p>
            <p class='brand'>Artistic_Sisters</p>
            <p class='tagline'>Bringing your cherished moments and visions to life through<br>traditional and realistic art.</p>
        </div>
        <div class='content-header'>
            <h1>{headerTitle}</h1>
        </div>
        <div class='content'>
            {contentHtml}
        </div>
        <div class='footer'>
            &copy; {DateTime.UtcNow.Year} Artistic Sisters. All rights reserved.
        </div>
    </div>
</body>
</html>";
    }
}
