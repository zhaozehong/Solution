   M S B A M L   `   `   `  ����  	HelperLib    TreeViewStyle\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089  �� MergedDictionaries �� Source   TreeViewItemFocusVisual �� Property �� Value �� 
TargetType  TransparentBrush �� 	Resources �� RelativeSource �� AncestorType �� 
TargetType	 � ColumnDefinitions
 � RowDefinitions d�Name �� 	ClickMode  ExpandCollapseToggleStyle d� 	IsChecked ��Name J� SnapsToDevicePixels �� Padding ��Name ��Name �� Triggers P� Property P� Value �� 
TargetName  SelectedItemBackground ]� 
Conditions �� Property �� Value :� RoutedEvent �� TargetProperty W� From	 W� To g� Duration �� Triggers   5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      :x,http://schemas.microsoft.com/winfx/2006/xaml     5        5      �� 5      $1 +/HelperLib;Component/Resources/Brushes.xaml?�6   �� 5      $< 6/HelperLib;Component/Resources/Styles/ButtonStyle.xaml?�6   %�	  &	        'R� n     7  7  'Q� �     7  7  7  �� 5	      .�5
      ��  � �� 5      �� 5   
   .R�5      �� 5      5      5      �� 5      " R�6
   .�5      ��  � ��8   6#   �� 5       � ��8  6$   5      �� 5      " Q�6
    5      ����5      (��       6   * SystemColors.HighlightBrushKey+)Transparent��6M   ��5      (��       6   *SystemColors.ControlBrushKey+)Transparent��6K   5      .�5      ��  � ��# � ��6#   �� 5       � ��8   6#   �� 5       � �� 63   �� *HorizontalContentAlignment+ �� " ���� 5       � �� 61   �� *VerticalContentAlignment+ �� " ���� 5        � ��$ Normal"�6#   �� 5!       � �� 5"      �� 5#   
   .R�5$      �� 5%      5'      �� 5(       � �� 5)      �� 5*   
   " Q�6   .R�5+      � 	 5,      �� 5-      $��19��6!   $
��Auto �6/   �� 5.      $
��Auto �6!   �� 5/      $��* �6!   
 51      �� 52      $
��Auto �6   �� 53      .+�56      d� $ Expander��6   $ Press=�6-   8��  58      �� *
IsExpanded+ �� *TemplatedParent+�� 59      $ Bd��6@   $��1��6    . #��z��5:      #��z��6A   #��z��6u   # z��6�   ._�5;      �� $ PART_Header��6!   $��Header��66   #��z��5<   !   # z 6d   5=      �� 5>      $ 	ItemsHost��6N   $��1��6   $��1��6*   $��2��6:   6N   5?       5@      P� 5A       �@Q�
IsExpanded .  . �5B      �� $ 	ItemsHost��6-    � y�$ 	Collapsed=�6D   5C      P� 5D       �@��HasItems .  . �5E      �� $ Expander��6-    � y�$ Hidden=�6C   5F      P� 5G       � {� .  . �5H      �� # � ��6-    � ��5I      P� 5J       �@Q�
IsSelected . . �5K      �� $ Bd��6-   8   � ��6=   5L      ]� 5M       5N      �� 5O       �@Q�
IsSelected . 62   �� 5P       �@Q�IsSelectionActive .  69   ./�5R      �� $ Bd��6-   # � ��6=    � ���� 5S      # � ��6-    � ��5T      :� 5U      $ DragOver��6   .[�5V      �� .��5W      �� .��5X      W� $ Opacity�6$   $	 0.4X�6H   $ 1X�6S   $ 0:0:0.5D�6Z   5Y      5Z      5[      5]      5_       5`      P� 5a       �@%�IsVirtualizing . . �5b   
   ��  � �� 5c      �� 5d      .R�5e      $� 5f      5h      5i   	   5k      5l      