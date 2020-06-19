#!/usr/bin/env python
# -*- encoding: utf-8 -*-
"""
@File    :   extractorUniPkg.py
@Time    :   2020/06/19 20:03:00
@Author  :   JunQiang
@Contact :   354888562@qq.com
@Desc    :   
"""

# here put the import lib
from unitypackage_extractor.extractor import extractPackage

UniPkgPath = r""
OutputPath = r""

extractPackage(UniPkgPath, outputPath=OutputPath)