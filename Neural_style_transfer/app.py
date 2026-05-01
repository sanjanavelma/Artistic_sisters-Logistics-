from flask import Flask, request, render_template, jsonify, send_from_directory
from nst_model import run_style_transfer
import os
import uuid

app = Flask(__name__)
UPLOAD_FOLDER = 'temp'
os.makedirs(UPLOAD_FOLDER, exist_ok=True)

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/stylize', methods=['POST'])
def stylize():
    content = request.files.get('content')
    style = request.files.get('style')
    alpha = float(request.form.get('intensity', 1.0))

    if not content or not style:
        return jsonify({'error': 'Missing content or style image'}), 400

 
    content_filename = f"{uuid.uuid4().hex[:8]}_{content.filename}"
    style_filename = f"{uuid.uuid4().hex[:8]}_{style.filename}"

    content_path = os.path.join(UPLOAD_FOLDER, content_filename)
    style_path = os.path.join(UPLOAD_FOLDER, style_filename)

    content.save(content_path)
    style.save(style_path)


    result_path = run_style_transfer(content_path, style_path, alpha)

    image_url = '/' + result_path.replace("\\", "/")
    return jsonify({'image_url': image_url})

@app.route('/temp/<filename>')
def serve_temp_file(filename):
    return send_from_directory(UPLOAD_FOLDER, filename)

if __name__ == '__main__':
    app.run(debug=True, port=5050)
