# 运动会分数统计系统设计思路

当咱第一次抽到这题的时候，其实咱是拒绝的……因为咱觉得，这题目不够有趣，不够神必，你不能让咱写，咱就去写，甚至网上本身就能搜到十万甚至九万个n年前的c程序代码。咱还看过有些代码，那个c++它是这样写的，lable，GO

经过咱简单的分析，本题是经典的关系数据库题（x），所以只需要按照关系数据库的思路来写大概就没有问题了（x）

## 关系模型

其中，最小实体集是存储数据所必须的实体属性集合，扩展实体集是用户可能查询的实体属性集合

### 最小实体集

|实体|属性名|属性类型|属性说明|
|---|---|---|---|
|school|id|int|学校编号|
|school|name|string|学校名称|
|student|id|int|运动员编号|
|student|name|string|名字|
|student|gender|char|性别|
|student|schoolid|int|所属学校
|event|id|int|运动项目编号|
|event|name|string|运动项目名称|
|event|gender|char|性别|
|event|grade|int array|前5名的赋分|
|event|students|int array|前5名运动员的id|

### 扩展实体集

|实体|属性名|属性类型|属性说明|
|---|---|---|---|
|school|count|int|学校获取名次的人数|
|school|score|int|学校总积分|
|school|rank|int|学校总积分排名|
|school|students|int array|学校获取名次的运动员id列表|
|school|events|int array|学校获取名次的项目id列表|
|student|score|int|运动员总积分|
|student|rank|int|运动员总积分排名|
|student|score|int array|运动员获取名次的项目id列表|
|event|count|int|运动项目参加人数|
|event|name|string|运动项目名称|
|event|gender|char|性别|
|event|grade|int array|前5名的赋分|
|event|students|int array|前5名运动员的id|
|event|students|int array|在运动项目中获取名次的学校id|

### 一些说明和完整性约束

- 学校编号从1开始
- 名次越高赋分越高
- 处于正数赋分之外的运动员属于冗余记录，不能存储与查询
- 基于系统的功能，并不需要存储成绩
- 如果一个人获取多项名次，学校获取名次的人数只计一次

## 筛选和排序

- 学校：
  - 筛选：编号/名次/积分+性别条件/项目条件
  - 排序：编号/名称/名次/积分 + 性别条件/项目条件
- 运动员：
  - 筛选：编号/名次 + 性别条件/项目条件
  - 排序：编号/姓名/名次/积分 + 项目条件？
- 项目
  - 筛选：编号/参加人数/赋分方式
  - 排序：编号/名称/参加人数

## 数据规模分析	

明天再写（

## 功能结构

### 基础功能

#### 建立数据文件

系统需要建立自己的数据文件

#### Load数据

系统需要从通用格式导入数据

目前计划允许XML格式数据导入，或许咱还要再学一次XML验证

#### 插入/删除/修改功能

当然，这些其实可以最后写

#### 所有查询功能

相信咱，这些才是最重要的

### 扩展功能

- 帮助、提示
- 优化存储方式
- 异常处理

## 运行程序结构

由一个控制台程序和若干个数据文件组成

## DSL语法词汇表

- Load(path)
- 查询
  1. student.filter(id<3).asc(id)
  2. student.filter(id<3).desc(id).skip(num).limit(num)
  3. 或者使用eq(id,3).regex(name,"学校").contain(name,"学校"）
  4. 查询结果超出一定行数不会显示？
  5. 导出文件：在最后加上.out()
- 插入
  - student.insert(id,name,..)
- 删除
  - student.filter().delete()

## 文件存储结构

明天再写（

## 测试单元

以后再写（

## 语言与平台选择

C# Windows控制台

## 关于本项目的参考价值

毕竟xy很菜，而且只是一个要在10天之内写完的一般课程设计，所以价值并不高
