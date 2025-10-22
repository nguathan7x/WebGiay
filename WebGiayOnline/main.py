import pyodbc
import pandas as pd
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity
from flask import Flask, jsonify, request

app = Flask(__name__)

# Thông tin SQL Server
server = 'DESKTOP-MJMDGVE\\SQLEXPRESS07'  # Đổi nếu khác
database = 'Giay'

connection_string = f'DRIVER={{SQL Server}};SERVER={server};DATABASE={database};Trusted_Connection=yes'

try:
    conn = pyodbc.connect(connection_string)
    query = 'SELECT * FROM Giays'
    df_sanpham = pd.read_sql(query, conn)

    print("✔ Đã kết nối SQL thành công.")
    print("Cột dữ liệu:", df_sanpham.columns)
    print("5 sản phẩm đầu tiên:\n", df_sanpham.head())

except Exception as e:
    print(f"❌ Lỗi kết nối: {e}")
    df_sanpham = pd.DataFrame()

finally:
    try:
        conn.close()
    except:
        pass

def combineFeatures(row):
    try:
        return str(row['Price']) + " " + str(row['Description'])
    except:
        return ""

if not df_sanpham.empty:
    df_sanpham['combinedFeatures'] = df_sanpham.apply(combineFeatures, axis=1)
    tf = TfidfVectorizer()
    tfMatrix = tf.fit_transform(df_sanpham['combinedFeatures'])
    similar = cosine_similarity(tfMatrix)
else:
    tfMatrix = None
    similar = None
@app.route('/api', methods=['GET'])
def get_data():
    if df_sanpham.empty or tfMatrix is None:
        return jsonify({'error': 'Không có dữ liệu'}), 500

    try:
        productid = int(request.args.get('id'))
    except:
        return jsonify({'error': 'ID không hợp lệ'}), 400

    if productid not in df_sanpham['GiayId'].values:
        return jsonify({'error': 'ID không tồn tại'}), 404

    indexproduct = df_sanpham[df_sanpham['GiayId'] == productid].index[0]
    similarProduct = list(enumerate(similar[indexproduct]))
    sortedSimilarProduct = sorted(similarProduct, key=lambda x: x[1], reverse=True)

    number = 4  # số lượng sản phẩm gợi ý
    result = []

    for i in range(1, number + 1):
        index = sortedSimilarProduct[i][0]
        sp = {
            "id": int(df_sanpham.iloc[index]['GiayId']),
            "name": df_sanpham.iloc[index]['Name'],
            "image_url": df_sanpham.iloc[index]['ImageUrl']  # Đổi tên cột nếu cần
        }
        result.append(sp)

    return jsonify({'san pham gui y': result})
if __name__ == '__main__':
     app.run(port=5555)

# @app.route('/api', methods=['GET'])
# def get_data():
#     if df_sanpham.empty or tfMatrix is None:
#         return jsonify({'error': 'Không có dữ liệu'}), 500

#     try:
#         productid = int(request.args.get('id'))
#     except:
#         return jsonify({'error': 'ID không hợp lệ'}), 400

#     if productid not in df_sanpham['GiayId'].values:
#         return jsonify({'error': 'ID không tồn tại'}), 404

#     indexproduct = df_sanpham[df_sanpham['GiayId'] == productid].index[0]
#     similarProduct = list(enumerate(similar[indexproduct]))
#     sortedSimilarProduct = sorted(similarProduct, key=lambda x: x[1], reverse=True)

#     def lay_ten(index):
#         return df_sanpham.iloc[index]['Name']

#     number = 4
#     KQua = []
#     for i in range(1, number + 1):
#         KQua.append(lay_ten(sortedSimilarProduct[i][0]))

#     return jsonify({'san pham gui y': KQua})

# if __name__ == '__main__':
#     app.run(port=5555)