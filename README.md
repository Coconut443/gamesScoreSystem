# 运动会分数统计系统设计要求

## 任务描述

参加运动会有n个学校，比赛分成**男子**和**女子**项目。不同的项目取前五名或前三名获得积分，例如，取前五名的得分分别为：7、5、3、2、1，前三名的得分分别为：5、3、2。其中，所有的编号都是**从1开始**且**连续**的

注：学校数、项目数都是确定的

## 功能要求

1. 可以输入各个项目的前三名或前五名的成绩。
2. 能统计各学校总分。
3. 可以按学校编号或名称、学校总分、男女团体总分排序输出。
4. 可以按学校编号查询学校某个项目的情况；可以按项目编号查询取得前三或前五名的学校。
5. 数据存入文件并能随时查询。
6. 规定：输入数据形式和范围：可以输入学校的名称，运动项目的名称。

## 输出形式

有合理的提示，各学校分数为整型

## 界面要求

有合理的提示，每个功能可以设立菜单，根据提示，可以完成相关的功能要求。

注意**只能使用控制台界面**

## 存储结构

学生自己根据系统功能要求自己设计，但是要求运动会的相关数据要存储在数据文件中。

## 测试数据

要求使用全部合法数据/整体非法数据/局部非法数据分别进行程序测试，以保证程序的稳定。测试数据及测试结果请在上交的资料中写明。

## 设计思路

见[design_ideas.md](/design_ideas.md)